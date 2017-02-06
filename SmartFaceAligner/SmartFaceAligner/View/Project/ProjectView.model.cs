using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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

namespace SmartFaceAligner.View.Project
{
    public class ProjectViewModel : ViewModel
    {
        private readonly IFileManagementService _fileManagementService;
        private readonly IProjectService _projectService;
        public Contracts.Entity.Project Project { get; set; }

        public ObservableCollection<FaceItemViewModel> FaceItems { get; private set; }

        public ICommand ImportCommand => Command(_import);

        public ProjectViewModel(IFileManagementService fileManagementService,
            IProjectService projectService)
        {
            _fileManagementService = fileManagementService;
            _projectService = projectService;
            FaceItems = new ObservableCollection<FaceItemViewModel>();
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

            FaceItemViewModel Wrap(string f)
            {
                var vm = Scope.Resolve<FaceItemViewModel>();
                vm.FileName = f;
                return vm;
            }

            files.ForEach(_ => FaceItems.Add(Wrap(_)));
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
        }
    }
}
