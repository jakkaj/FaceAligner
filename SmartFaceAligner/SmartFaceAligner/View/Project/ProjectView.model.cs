using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

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
using System.Diagnostics;
using Microsoft.ProjectOxford.Face.Contract;
using SmartFaceAligner.Processor.Entity;

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

        private FaceItemViewModel _selectedItem;

        private FaceItemViewModel _selectedFace;

        public ICommand ImportCommand => Command(_import);
        public ICommand AddNewIdentityGroupCommand => Command(_addNewIdentityGroup);
        public ICommand TrainCommand => Command(_train);

        public ICommand FilterFacesCommand => Command(_filterFaces);

        public ICommand FilterMalesCommand => Command(_filterMales);
        public ICommand FilterFemalesCommand => Command(_filterFemales);

        public ICommand DetectFacesCommand => Command(_detectFaces);
        public ICommand RunFilterCommand => Command(_runFilter);
        public ICommand ClearFilterCommand => Command(_clearFilter);
        public ICommand SortByAgeCommand => Command(_sortByAge);
        public ICommand AlignCommand => Command(_align);
        

       

        private RecognisePersonConfigViewModel _currentIdentity;
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

        public RecognisePersonConfigViewModel CurrentIdentity
        {
            get { return _currentIdentity; }
            set
            {
                _currentIdentity = value;
                OnPropertyChanged();
            }
        }

        public FaceItemViewModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
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

            if (_currentIdentity == null)
            {
                return;
            }

            foreach (var f in SelectedItems)
            {
                var faceData = f.FaceData;
                await _projectService.AddImageToPerson(_currentIdentity.IdentityPerson, faceData);
            }


            _loadIdentities();

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

        private async void _train()
        {
            var messageBoxResult = System.Windows.MessageBox.Show($"Warning. This will clear all previous detection results. Continue?", "Clear previous detectio results", System.Windows.MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.No)
            {
                return;
            }

            await _projectService.ClearIdentity(Project);
            await _faceService.TrainPersonGroups(Project);
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

            return face.IdentityPerson != null && face.IdentityPerson.PersonId == _currentIdentity.IdentityPerson.PersonId;
        }

        bool _filterFaceItemVmByIdentity(FaceItemViewModel vm)
        {
            if (_currentIdentity == null)
            {
                return true;
            }

            if (vm.FaceData == null || vm.FaceData.ParsedFaces == null)
            {
                return false;
            }

            var any = vm.FaceData.ParsedFaces.Any(_ => _.IdentityPerson != null && _.IdentityPerson.PersonId == _currentIdentity.IdentityPerson.PersonId);

            if (any)
            {
                Debug.WriteLine("found");
            }

            return any;
        }

        async void _clearFilter()
        {
            CurrentIdentity = null;
            await Load();
        }

        public ICommand FilterByReadingGlassesCommand => Command(_filterByReadingGlasses);
        public ICommand FilterBySunGlassesCommand => Command(_filterBySunGlasses);
        public ICommand FilterByNoGlassesCommand => Command(_filterByNoGlasses);
        public ICommand FilterByGogglesCommand => Command(_filterBySwimmingGoggles);

        void _filterByReadingGlasses()
        {
            _filterByGlasses(Glasses.ReadingGlasses);
        }

        void _filterBySunGlasses()
        {
            _filterByGlasses(Glasses.Sunglasses);
        }

        void _filterByNoGlasses()
        {
            _filterByGlasses(Glasses.NoGlasses);
        }

        void _filterBySwimmingGoggles()
        {
            _filterByGlasses(Glasses.SwimmingGoggles);
        }

        private async void _filterByGlasses(Glasses type)
        {
            await Load();
            var fTemp = FaceItems.ToList();

            var filtered = fTemp.Where(
                _ => _.FaceData.ParsedFaces != null &&
                _.FaceData.ParsedFaces.Any(
                    _2 => _2.Face.FaceAttributes.Glasses == type)
                ).ToList();

            FaceItems.Clear();

            filtered.ForEach(_ => FaceItems.Add(_));
        }

        private async void _filterByGender(string gender)
        {
            await Load();
            var fTemp = FaceItems.ToList();

            var filtered = fTemp.Where(
                _=> _.FaceData.ParsedFaces != null && 
                _.FaceData.ParsedFaces.Any(
                    _2=>_2.Face.FaceAttributes.Gender == gender)
                ).ToList();

            FaceItems.Clear();

            filtered.ForEach(_ => FaceItems.Add(_));
        }

        void _filterMales()
        {
            _filterByGender(Constants.Filters.Male);
        }

        void _filterFemales()
        {
            _filterByGender(Constants.Filters.Female);
        }



        private async void _runFilter()
        {
            await Load();
            var fTemp = FaceItems.ToList();

            var filtered = fTemp.Where(_filterFaceItemVmByIdentity).ToList();

            FaceItems.Clear();

            filtered.ForEach(_ => FaceItems.Add(_));
        }

        private async void _detectFaces()
        {
            async Task FilterLocal(FaceData faceData)
            {
                await _faceService.CognitiveDetectFace(Project, faceData);
            }

            var tasks = new Queue<Func<Task>>();

            foreach (var f in FaceItems)
            {
                if (f.FaceData.HasBeenScanned)
                {
                    if (f.FaceData.ParsedFaces != null && f.FaceData.ParsedFaces.Length > 0)
                    {
                        //can do some checking here for debugging. 
                    }
                }
                else
                {
                    tasks.Enqueue(() => FilterLocal(f.FaceData));
                }
            }

            await tasks.Parallel(10);

            await Load();

            System.Windows.MessageBox.Show("Face detection complete", "Processing",  MessageBoxButton.OK);

;        }

        /// <summary>
        /// this does a local filter using EMGU face detection. Can be useful to pre-clean, but may not work as well as other systems. 
        /// </summary>
        async void _filterFaces()
        {
            await Load();//reload them so they are all there incase it's a new face they want to filter on. 

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
               var files = await _fileManagementService.GetSourceFiles(Project);
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
            var selId = _currentIdentity?.IdentityPerson?.PersonName;

            IdentityPeople.Clear();
           
            CurrentIdentity = null;
            foreach (var id in Project.IdentityPeople)
            {
                var vm = Scope.Resolve<RecognisePersonConfigViewModel>();
                vm.Project = Project;
                vm.IdentityPerson = id;
                IdentityPeople.Add(vm);
            }

            if (selId != null)
            {
                CurrentIdentity = IdentityPeople.FirstOrDefault(_ => _.IdentityPerson.PersonName == selId);
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

            if (result != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }

            var count = await _fileManagementService.ImportFolder(Project, dialog.SelectedPath);
            System.Windows.MessageBox.Show($"Import Complete. {count} files imported.", "Importing", System.Windows.MessageBoxButton.OK);
            await Load();
        }


    }
}
