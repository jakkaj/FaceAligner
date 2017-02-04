using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCoreLite.View;

namespace SmartFaceAligner.View.NewProject
{
    public class NewProjectStartViewModel : ViewModel
    {
        private string _projectName;

        public NewProjectStartViewModel()
        {
            
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
