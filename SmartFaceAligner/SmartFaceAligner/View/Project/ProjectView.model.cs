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
        public IList<FaceItemViewModel> SelectedItems { get; set; }

        private FaceItemViewModel _selectedFace;

        public ICommand ImportCommand => Command(_import);
        public ICommand FilterFacesCommand => Command(_filterFaces);
        public ICommand FilterPersonCommand => Command(_filterPersonCommand);
        public ICommand RunFilterCommand => Command(_runFilter);
        public ICommand SortByAgeCommand => Command(_sortByAge);
        public ICommand AlignCommand => Command(_align);
       

        public FaceItemViewModel SelectedFace
        {
            get { return _selectedFace; }
            set
            {
                _selectedFace = value;
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

            this.Register<ViewItemMessage>(_onViewPortUpdatedMessage);
        }

        public async void SelectFilterPersonCommand()
        {
            if (SelectedItems == null)
            {
                return;
            }
            await _faceService.SetPersonGroupPhotos(Project, SelectedItems.Select(_ => _.FaceData).ToList());
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

        private async void _align()
        {
            await _faceService.PrepAlign(Project);
            async Task FilterLocal(FaceData faceData)
            {
                await _faceService.Align(Project, FaceItems.First().FaceData, faceData);
            }

            var tasks = new Queue<Func<Task>>();

            foreach (var f in FaceItems.Where(_ => _.FaceData.Face != null))
            {
                tasks.Enqueue(() => FilterLocal(f.FaceData));
            }

            await tasks.Parallel(4);
        }

        private void _sortByAge()
        {
            var fTemp = FaceItems.OrderBy(_ => _.FaceData.Face.FaceAttributes.Age);

            var filtered = fTemp.Where(_ => _.FaceData.Face != null).ToList();

            FaceItems.Clear();

            filtered.ForEach(_ => FaceItems.Add(_));
        }

        private void _runFilter()
        {
            var fTemp = FaceItems.ToList();

            var filtered = fTemp.Where(_ => _.FaceData.Face != null).ToList();

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

            foreach (var f in FaceItems.Where(_=>_.FaceData.Face == null))
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

            var filtered = fTemp.Where(_ => _.FaceData.HasFace.HasValue && _.FaceData.HasFace.Value).ToList();

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
            var files = await _fileManagementService.GetFiles(Project, ProjectFolderTypes.Staging);
            FaceItems.Clear();

            async Task Wrap(string f)
            {
                var vm = Scope.Resolve<FaceItemViewModel>();
                vm.FaceData = await _faceDataService.GetFaceData(Project, f);
                FaceItems.Add(vm);
            }

            await files.WhenAllList(_=>Wrap(_));
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
