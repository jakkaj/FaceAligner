using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Contracts.Interfaces;
using XCoreLite.View;

namespace SmartFaceAligner.View.Project
{
    public class NewIdentityViewModel : ViewModel
    {
        private readonly IProjectService _projectService;
        public Contracts.Entity.Project Project { get; set; }
        private string _createname;

        public ICommand CreateCommand => Command(_create);

        public string Createname
        {
            get { return _createname; }
            set
            {
                _createname = value;
                OnPropertyChanged();
            }
        }

        public NewIdentityViewModel(IProjectService projectService)
        {
            _projectService = projectService;
        }

        private async void _create()
        {
            if (string.IsNullOrWhiteSpace(Createname))
            {
                return;
            }

            await _projectService.AddNewIdentityPerson(Project, Createname);
            NavigateBack();
        }
    }
}
