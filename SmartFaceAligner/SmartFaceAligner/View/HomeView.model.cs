using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using SmartFaceAligner.View.Setup;
using XCoreLite.View;
using Contracts;
using Contracts.Interfaces;
using System.Windows.Forms;
using SmartFaceAligner.Processor.Entity;
using SmartFaceAligner.View.NewProject;
using SmartFaceAligner.View.Project;

namespace SmartFaceAligner.View
{
    public class HomeViewModel : ViewModel
    {
        private readonly IProjectService _projectService;
        private readonly IConfigurationService _configService;

        public ICommand NewProjectCommand => Command(_newProject);
        public ICommand OpenProjectCommand => Command(_existingProject);

        public HomeViewModel(IProjectService projectService, IConfigurationService configService)
        {
            _projectService = projectService;
            _configService = configService;
        }
       
        private async void _existingProject()
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "Json Documents (.json)|*.json",
                DefaultExt = ".json"
            };

            if (openFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            var f = openFileDialog.FileName;

            var project = await _projectService.OpenProject(f);

            await NavigateTo<ProjectViewModel>(_ => _.Project = project);

        }

        private async void _newProject()
        {
            await NavigateTo<NewProjectStartViewModel>();
        }

        public override async Task NavigatedTo(bool isBack)
        {
            if (_configService.NeedsConfig && !isBack)
            {
                await Task.Delay(1000);
                await NavigateTo<SetupViewModel>();
            }
        }
    }
}
