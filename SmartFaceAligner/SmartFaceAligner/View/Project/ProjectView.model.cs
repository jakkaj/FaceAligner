using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Autofac;
using Contracts.Entity;
using Contracts.Interfaces;
using ExtensionGoo.Standard.Extensions;
using SmartFaceAligner.View.Face;
using XCoreLite.View;
using SmartFaceAligner.Messages;
using SmartFaceAligner.Util;
using XamlingCore.Portable.Messages.XamlingMessenger;
using XamlingCore.Portable.Util.TaskUtils;

namespace SmartFaceAligner.View.Project
{
    public class ProjectViewModel : ViewModel
    {
        private readonly IFileManagementService _fileManagementService;
        private readonly IProjectService _projectService;
        private readonly IFaceDataService _faceDataService;
        private readonly IFaceService _faceService;

        public Contracts.Entity.Project Project { get; set; }

        public ObservableCollection<FaceItemViewModel> FaceItems { get; private set; }
        public ObservableCollection<RecognisePersonConfigViewModel> IdentityPeople {get;private set;}
        public IList<FaceItemViewModel> SelectedItems { get; set; }

        private FaceItemViewModel _selectedFace;

        public ICommand ImportCommand => Command(_import);
        public ICommand AddNewIdentityGroupCommand => Command(_addNewIdentityGroup);
        public ICommand FilterFacesCommand => Command(_filterFaces);
        public ICommand FilterPersonCommand => Command(_filterPersonCommand);
        public ICommand RunFilterCommand => Command(_runFilter);
        public ICommand SortByAgeCommand => Command(_sortByAge);
        public ICommand AlignCommand => Command(_align);

        private IdentityPerson _currentIdentity;
        private FaceData _alignFaceData;

        public FaceItemViewModel SelectedFace
        {
            get { return _selectedFace; }
            set
            {
                _selectedFace = value;
                OnPropertyChanged();
            }
        }

        public IdentityPerson CurrentIdentity
        {
            get { return _currentIdentity; }
            set
            {
                _currentIdentity = value;
                OnPropertyChanged();
            }
        }

        public ProjectViewModel(IFileManagementService fileManagementService,
            IProjectService projectService,
            IFaceDataService faceDataService, 
            IFaceService faceService)
        {
            _fileManagementService = fileManagementService;
            _projectService = projectService;
            _faceDataService = faceDataService;
            _faceService = faceService;
            FaceItems = new ObservableCollection<FaceItemViewModel>();
            IdentityPeople = new ObservableCollection<RecognisePersonConfigViewModel>();

            this.Register<ViewItemMessage>(_onViewPortUpdatedMessage);
        }

        public async void SelectFilterPersonCommand()
        {
            if (SelectedItems == null)
            {
                return;
            }
            await _faceService.TrainPersonGroups(Project);
        }

        void _onViewPortUpdatedMessage(object message)
        {
            if (message is ViewItemMessage m)
            {
                var offset = m.Offset;
                var width = m.Width;

                var selIndex = Convert.ToInt32(offset + (width / 2));
                if (selIndex > FaceItems.Count - 1)
                {
                    selIndex = FaceItems.Count - 1;
                }

                SelectedFace = FaceItems[selIndex];

            }
        }

        async void _addNewIdentityGroup()
        {
            await NavigateTo<NewIdentityViewModel>(_=>_.Project = Project);
        }

        private async void _align()
        {
            if (_currentIdentity == null || _alignFaceData == null)
            {
                return;
            }

            await _faceService.PrepAlign(Project);

            var _alignFace = _alignFaceData.ParsedFaces.FirstOrDefault(_filterParsedFaceByIdentity)?.Face;

            if (_alignFace == null)
            {
                return;
            }

             async Task FilterLocal(FaceData faceData)
            {
                var thisAlignFace = faceData.ParsedFaces.FirstOrDefault(_filterParsedFaceByIdentity)?.Face;
                if (thisAlignFace == null)
                {
                    return;
                }
                await _faceService.Align(Project,_alignFaceData,faceData, _alignFace, thisAlignFace);
            }

            var tasks = new Queue<Func<Task>>();

            foreach (var f in FaceItems.Where(_filterFaceItemVmByIdentity))
            {
                tasks.Enqueue(() => FilterLocal(f.FaceData));
            }

            await tasks.Parallel(4);
        }

        private void _sortByAge()
        {
            if (_currentIdentity == null)
            {
                return;
            }

            var fTemp =
                FaceItems.OrderBy(
                    _ =>
                        _.FaceData.ParsedFaces.FirstOrDefault(_filterParsedFaceByIdentity)?
                            .Face.FaceAttributes.Age).ToList();

            FaceItems.Clear();

            fTemp.ForEach(_ => FaceItems.Add(_));
        }


        bool _filterParsedFaceByIdentity(ParsedFace face)
        {
            if (_currentIdentity == null)
            {
                return true;
            }

            return face.IdentityPerson != null && face.IdentityPerson.PersonId == _currentIdentity.PersonId;
        }

        bool _filterFaceItemVmByIdentity(FaceItemViewModel vm)
        {
            if (_currentIdentity == null)
            {
                return true;
            }

            return vm.FaceData?.ParsedFaces != null &&
                   vm.FaceData.ParsedFaces.Any(_ => _.IdentityPerson.PersonId == _currentIdentity.PersonId);
        }

        private void _runFilter()
        {
            var fTemp = FaceItems.ToList();

            var filtered = fTemp.Where(_filterFaceItemVmByIdentity).ToList();

            FaceItems.Clear();

            filtered.ForEach(_ => FaceItems.Add(_));
        }

        private async void _filterPersonCommand()
        {
            async Task FilterLocal(FaceData faceData)
            {
                await _faceService.CognitiveDetectFace(Project, faceData);
            }

            var tasks = new Queue<Func<Task>>();

            foreach (var f in FaceItems.Where(_=>!_.FaceData.HasBeenScanned ))
            {
                tasks.Enqueue(()=> FilterLocal(f.FaceData));
            }

            await tasks.Parallel(4);

;        }

        /// <summary>
        /// this does a local filter using EMGU face detection. Can be useful to pre-clean, but may not work as well as other systems. 
        /// </summary>
        async void _filterFaces()
        {
            await Task.Run(() =>
            {
                _faceService.LocalDetectFaces(FaceItems.Select(_ => _.FaceData).ToList());
            }).ConfigureAwait(true);

            var fTemp = FaceItems.ToList();

            var filtered = fTemp.Where(_ => !_.FaceData.HasBeenScanned).ToList();

            FaceItems.Clear();

            filtered.ForEach(_=>FaceItems.Add(_));
        }

        public override Task NavigatedTo(bool isBack)
        {
            Load();
            return base.NavigatedTo(isBack);
        }

        public async Task Load()
        {
            CurrentIdentity = null;

               var files = await _fileManagementService.GetFiles(Project, ProjectFolderTypes.Staging);
            FaceItems.Clear();

            async Task Wrap(string f)
            {
                var vm = Scope.Resolve<FaceItemViewModel>();
                vm.FaceData = await _faceDataService.GetFaceData(Project, f);
                FaceItems.Add(vm);
            }

            await files.WhenAllList(_=>Wrap(_));

            _loadIdentities();
        }

         void _loadIdentities()
        {
            IdentityPeople.Clear();

            foreach (var id in Project.IdentityPeople)
            {
                var vm = Scope.Resolve<RecognisePersonConfigViewModel>();
                vm.Project = Project;
                vm.IdentityPerson = id;
                IdentityPeople.Add(vm);
            }
        }

        async void _import()
        {
            if (await _fileManagementService.HasFiles(Project, ProjectFolderTypes.Staging))
            {
                var messageBoxResult = System.Windows.MessageBox.Show($"This project already has images imported. Delete them?", "Delete Confirmation", System.Windows.MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    await _fileManagementService.DeleteFiles(Project, ProjectFolderTypes.Staging);
                }
            }

            var dialog = new System.Windows.Forms.FolderBrowserDialog();

            var result = dialog.ShowDialog();

            if (result != DialogResult.OK)
            {
                return;
            }

            var count = await _fileManagementService.ImportFolder(Project, dialog.SelectedPath);
            System.Windows.MessageBox.Show($"Import Complete. {count} files imported.", "Importing", System.Windows.MessageBoxButton.OK);
            await Load();
        }


    }
}
