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
using System.Threading;
using Microsoft.ProjectOxford.Face.Contract;
using SmartFaceAligner.Processor;
using SmartFaceAligner.Processor.Entity;
using SmartFaceAligner.UtilVm;

namespace SmartFaceAligner.View.Project
{
    public class ProjectViewModel : ViewModel
    {
        private readonly IFileManagementService _fileManagementService;
        private readonly IProjectService _projectService;
        private readonly IFaceDataService _faceDataService;
        private readonly IFaceService _faceService;
        private readonly ILogService _logService;

        public Contracts.Entity.Project Project { get; set; }

        public ObservableCollection<FaceItemViewModel> FaceItems { get; private set; }
        public ObservableCollection<RecognisePersonConfigViewModel> IdentityPeople {get;private set;}
        public IList<FaceItemViewModel> SelectedItems { get; set; }

        private FaceItemViewModel _selectedItem;

        private FaceItemViewModel _selectedFace;

        public ICommand ImportCommand => Command(_import);
        public ICommand AddNewIdentityGroupCommand => Command(_addNewIdentityGroup);
        public ICommand TrainCommand => Command(_train);

        public FaceFilterViewModel Filter { get; set; }

        public ICommand DetectFacesCommand => Command(_detectFaces);
        public ICommand RunFilterCommand => Command(_runFilter);
        public ICommand ClearFilterCommand => Command(_clearFilter);
        public ICommand SortByAgeCommand => Command(_sortByAge);
        public ICommand AlignCommand => Command(_align);

        public ICommand SaveFilteredItemsCommand => Command(_saveCurrentImages);

        private CancellationTokenSource _alignCancel;

        string _currentLog;

        public FaceItemViewModel SelectedFace
        {
            get { return _selectedFace; }
            set
            {
                _selectedFace = value;
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
            IFaceService faceService,
            ILogService logService)
        {
            _fileManagementService = fileManagementService;
            _projectService = projectService;
            _faceDataService = faceDataService;
            _faceService = faceService;
            _logService = logService;
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

            var selectedPeople = _selectedPeople();

            if (selectedPeople.Count == 0 || selectedPeople.Count > 1)
            {
                return;
            }

            var currentIdentity = selectedPeople.FirstOrDefault();

            foreach (var f in SelectedItems)
            {
                var faceData = f.FaceData;
                await _projectService.AddImageToPerson(currentIdentity, faceData);
            }


            _loadIdentities();

        }

        List<IdentityPerson> _selectedPeople()
        {
            if (IdentityPeople == null)
            {
                return new List<IdentityPerson>();
            }
            var lResult = IdentityPeople.Where(_ => _.IsChecked).Select(_ => _.IdentityPerson).ToList();
            return lResult;
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
                if (selIndex < 0)
                {
                    return;
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
            if (_alignCancel != null)
            {
                _alignCancel.Cancel();
                _alignCancel = null;
                System.Windows.MessageBox.Show($"Alignment Cancelled", "Aligner", System.Windows.MessageBoxButton.OK);
                return;
            }

           
            var selectedPeople = _selectedPeople();

            var currentIdentity = selectedPeople.FirstOrDefault();

            if (currentIdentity == null || SelectedFace?.FaceData == null)
            {
                return;
            }

            await _faceService.PrepAlign(Project);

            var alignFace = SelectedFace.FaceData.ParsedFaces.FirstOrDefault(_filterParsedFaceByIdentity);

            var a2 = alignFace;
            if (a2 == null)
            {
                return;
            }

            _alignCancel = new CancellationTokenSource();

            async Task FilterLocal(FaceData faceData)
            {
                var thisAlignFace = faceData.ParsedFaces.FirstOrDefault(_filterParsedFaceByIdentity)?.Face;
                if (thisAlignFace == null)
                {
                    return;
                }
                await _faceService.Align(Project,SelectedFace.FaceData,faceData, alignFace.Face, thisAlignFace);
            }

            var tasks = new Queue<Func<Task>>();

            foreach (var f in FaceItems.Where(_filterRunner))
            {
                tasks.Enqueue(() => FilterLocal(f.FaceData));
            }
            try
            {
                await tasks.Parallel(4, _alignCancel.Token);
                _alignCancel = null;
                await _faceService.PostAlign(Project);
                System.Windows.MessageBox.Show($"Alignment Completed", "Aligner", System.Windows.MessageBoxButton.OK);
            }
            catch(Exception ex)
            {
                _logService.Log(ex.ToString());
            }
          
        }

        private void _sortByAge()
        {
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
            var selectedPeople = _selectedPeople();

            var selectedId = selectedPeople.Select(_ => _.PersonId).ToArray();
           
            return face.IdentityPerson != null && selectedId.Contains(face.IdentityPerson.PersonId);
        }

        bool _filterRunner(FaceItemViewModel vm)
        {
            if (vm.FaceData == null || vm.FaceData.ParsedFaces == null)
            {
                return false;
            }

            var selectedPeople = _selectedPeople();

            if (selectedPeople.Count != 0)
            {
                var selectedId = selectedPeople.Select(_ => _.PersonId).ToArray();

                bool containsAll = true;

                foreach (var id in selectedId)
                {
                    var any = vm.FaceData.ParsedFaces.Any(_ => _.IdentityPerson != null && _.IdentityPerson.PersonId == id);
                    if (!any)
                    {
                        containsAll = false;
                        break;
                    }
                }

                if (!containsAll)
                {
                    return false;
                }
            }

            //this contains all people or there were none selected. 

            var filterBase = vm.FaceData;

            if (Filter.Faces)
            {
                if (filterBase.ParsedFaces == null)
                {
                    return false;
                }
            }

            if (Filter.Females)
            {
                var filtered = filterBase.ParsedFaces.Any(_ => _.Face.FaceAttributes.Gender == Constants.Filters.Female);
                if (!filtered)
                {
                    return false;
                }
            }

            if (Filter.Males)
            {
                var filtered = filterBase.ParsedFaces.Any(_ => _.Face.FaceAttributes.Gender == Constants.Filters.Male);
                if (!filtered)
                {
                    return false;
                }
            }

            if (Filter.Goggles)
            {
                var filtered = filterBase.ParsedFaces.Any
                    (_ => _.Face.FaceAttributes.Glasses == 
                    Glasses.SwimmingGoggles);
                if (!filtered)
                {
                    return false;
                }
            }

            if (Filter.NoGlasses)
            {
                var filtered = filterBase.ParsedFaces.Any
                    (_ => _.Face.FaceAttributes.Glasses ==
                    Glasses.NoGlasses);
                if (!filtered)
                {
                    return false;
                }
            }

            if (Filter.ReadingGlasses)
            {
                var filtered = filterBase.ParsedFaces.Any
                    (_ => _.Face.FaceAttributes.Glasses ==
                    Glasses.ReadingGlasses);
                if (!filtered)
                {
                    return false;
                }
            }

            if (Filter.Sunglasses)
            {
                var filtered = filterBase.ParsedFaces.Any
                    (_ => _.Face.FaceAttributes.Glasses ==
                    Glasses.Sunglasses);
                if (!filtered)
                {
                    return false;
                }
            }

            if (Filter.NotSmiling)
            {
                var filtered = filterBase.ParsedFaces.Any
                    (_ => _.Face.FaceAttributes.Smile <= .3);
                if (!filtered)
                {
                    return false;
                }
            }

            if (Filter.Smiling)
            {
                var filtered = filterBase.ParsedFaces.Any
                    (_ => _.Face.FaceAttributes.Smile >= .7);
                if (!filtered)
                {
                    return false;
                }
            }


            return true;

        }

        async void _clearFilter()
        {
            if (IdentityPeople != null)
            {
                foreach (var person in IdentityPeople)
                {
                    person.IsChecked = false;
                }
            }

            await Load();
        }

       

        public string CurrentLog
        {
            get { return _currentLog; }
            set
            {
                _currentLog = value; 
                OnPropertyChanged();
            }
        }

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
                _.FaceData.ParsedFaces.All(
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

        void _filterBySmiles()
        {
            var fTemp = FaceItems.ToList();

            var filtered = fTemp.Where(
                _ => _.FaceData.ParsedFaces != null &&
                _.FaceData.ParsedFaces.Any(
                    _2 => _2.Face.FaceAttributes.Smile > .7)
                ).ToList();

            FaceItems.Clear();

            filtered.ForEach(_ => FaceItems.Add(_));
        }

        void _filterByNotSmiles()
        {
            var fTemp = FaceItems.ToList();

            var filtered = fTemp.Where(
                _ => _.FaceData.ParsedFaces != null &&
                _.FaceData.ParsedFaces.Any(
                    _2 => _2.Face.FaceAttributes.Smile < .2)
                ).ToList();

            FaceItems.Clear();

            filtered.ForEach(_ => FaceItems.Add(_));
        }

        void _filterByFaces()
        {
            var fTemp = FaceItems.ToList();

            var filtered = fTemp.Where(
                _ => _.FaceData.ParsedFaces != null).ToList();

            FaceItems.Clear();

            filtered.ForEach(_ => FaceItems.Add(_));
        }

        private async void _runFilter()
        {
            await Load();
            var fTemp = FaceItems.ToList();

            var filtered = fTemp.Where(_filterRunner).ToList();

            FaceItems.Clear();

            filtered.ForEach(_ => FaceItems.Add(_));
        }

        async void _saveCurrentImages()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();

            var result = dialog.ShowDialog();

            if (result != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }

            await _fileManagementService.SaveFaces(FaceItems.Select(_=>_.FaceData).ToList(), dialog.SelectedPath);
        }

        private long _totalBytesSent = 0;
        private int _sentToServer = 0;
        private int _notSentToServer = 0;

        private async void _detectFaces()
        {

            if (_alignCancel != null)
            {
                _alignCancel.Cancel();
                _alignCancel = null;
                return;
            }

            _totalBytesSent = 0;
            _sentToServer = 0;
            _notSentToServer = 0;

            var tasks = new Queue<Func<Task>>();

            var count = 0;

            async Task FilterLocal(FaceData faceData)
            {
              (bool sent, long bytes) = await _faceService.CognitiveDetectFace(Project, faceData);
                if (sent)
                {
                    _sentToServer += 1;
                    _totalBytesSent += bytes;
                }
                else
                {
                    _notSentToServer += 1;
                }
                _logService.Log($"Cognitive Progress: {_totalBytesSent/1024/1024} mb / {_sentToServer} files sent. {_notSentToServer} ignored. {tasks.Count} remaining of {count} total.");
            }

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
            _logService.Log($"Processing {tasks.Count} items");
            count = tasks.Count;
            try
            {
                _alignCancel = new CancellationTokenSource();
                await tasks.Parallel(8, _alignCancel.Token);

                await Load();
            }
            catch (OperationCanceledException ex)
            {
                System.Windows.MessageBox.Show("Face detection cancelled", "Processing", MessageBoxButton.OK);
                return;
            }
            catch (Exception ex)
            {
                _logService.LogException(ex);
                System.Windows.MessageBox.Show($"Face detection exception :( {ex.ToString()}", "Processing", MessageBoxButton.OK);
            }
           

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
            Filter.PropertyChanged -= Filter_PropertyChanged;
            Filter.PropertyChanged += Filter_PropertyChanged;
            
            Load();
            _logService.Logged += _logService_Logged;
            return base.NavigatedTo(isBack);
        }

        private void Filter_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _runFilter();   
        }

        public override Task NavigatingAway(bool isBack)
        {
            _logService.Logged -= _logService_Logged;
            return base.NavigatingAway(isBack);
        }

        private async void _logService_Logged(object sender, TextEventArgs e)
        {
            if (e.Text.StartsWith("Cognitive Progress:") || e.Text.StartsWith("Reading:"))
            {
                CurrentLog = e.Text;
            }

            await Task.Yield();
        }

        public async Task Load()
        {
               //var files = await _fileManagementService.GetSourceFiles(Project);
            FaceItems.Clear();

            var faceItems = await _faceDataService.GetFaceData(Project);

            foreach (var fd in faceItems)
            {
                var vm = Scope.Resolve<FaceItemViewModel>();
                vm.FaceData = fd;
                FaceItems.Add(vm);
            }


            _loadIdentities();

            CurrentLog = $"Loaded {faceItems.Count} images";
        }

         void _loadIdentities()
         {
             var selId = _selectedPeople().Select(_ => _.PersonId).ToArray();

            IdentityPeople.Clear();
            
            foreach (var id in Project.IdentityPeople)
            {
                var vm = Scope.Resolve<RecognisePersonConfigViewModel>();
                vm.Project = Project;
                vm.IdentityPerson = id;
                IdentityPeople.Add(vm);
            }

            if (selId != null)
            {
                var lSeelctedReset = IdentityPeople.Where(_ => selId.Contains(_.IdentityPerson.PersonId)).ToList();
                foreach (var selectedPerson in lSeelctedReset)
                {
                    selectedPerson.IsChecked = true;
                }
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
