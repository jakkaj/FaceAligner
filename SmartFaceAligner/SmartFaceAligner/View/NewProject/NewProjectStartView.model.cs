using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using Contracts.Interfaces;
using SmartFaceAligner.View.Project;
using XCoreLite.View;

namespace SmartFaceAligner.View.NewProject
{
    public class NewProjectStartViewModel : ViewModel
    {
        private readonly IProjectService _projectService;
        private string _projectName;

        private string _sourceDir;

        public ICommand CreateCommand => Command(_create);

        public ICommand SetSourceDirectoryCommand => Command(_onSetSourceDirectory);

        public NewProjectStartViewModel(IProjectService projectService)
        {
            _projectService = projectService;
        }

        private async void _onSetSourceDirectory()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();

            var result = dialog.ShowDialog();

            if (result != DialogResult.OK)
            {
                return;
            }

            var path = dialog.SelectedPath;

            SourceDir = path;
        }

        private async void _create()
        {
            if (string.IsNullOrWhiteSpace(ProjectName))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(SourceDir))
            {
                return;
            }

            var d = new DirectoryInfo(SourceDir);
            if (!d.Exists)
            {
                MessageBox.Show("Could not find that source dir!");

                return;
            }

            var dialog = new System.Windows.Forms.FolderBrowserDialog();

            var result = dialog.ShowDialog();

            if (result != DialogResult.OK)
            {
                return;
            }

            var path = dialog.SelectedPath;

            var newProject = await _projectService.CreateProject(ProjectName, path, SourceDir);

            await NavigateTo<ProjectViewModel>(_ => _.Project = newProject);

        }

        public string ProjectName
        {
            get { return _projectName; }
            set
            {
                _projectName = value;
                OnPropertyChanged();
            }
        }

        public string SourceDir
        {
            get { return _sourceDir; }
            set
            {
                _sourceDir = value;
                OnPropertyChanged();
            }
        }
    }
}
