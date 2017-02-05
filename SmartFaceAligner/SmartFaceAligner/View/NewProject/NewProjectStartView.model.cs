using System;
using System.Collections.Generic;
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

        public ICommand CreateCommand => Command(_create);

        public NewProjectStartViewModel(IProjectService projectService)
        {
            _projectService = projectService;
        }

        private async void _create()
        {
            if (string.IsNullOrWhiteSpace(ProjectName))
            {
                return;
            }

            var dialog = new System.Windows.Forms.FolderBrowserDialog();

            var result = dialog.ShowDialog();

            if (result != DialogResult.OK)
            {
                return;
            }

            var path = dialog.SelectedPath;

            var newProject = await _projectService.CreateProject(ProjectName, path);

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
    }
}
