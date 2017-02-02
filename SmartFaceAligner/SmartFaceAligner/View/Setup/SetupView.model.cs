using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SmartFaceAligner.Processor.Contract;
using XCoreLite.View;

namespace SmartFaceAligner.View.Setup
{
    public class SetupViewModel : ViewModel
    {
        private readonly IConfigurationService _confService;

        public ICommand AzureLinkCommand => Command(_navToAzureSignup);
        public ICommand CogServicesLinkCommand => Command(_navToCogServicesPage);

        public SetupViewModel(IConfigurationService confService)
        {
            _confService = confService;
        }

        void _navToAzureSignup()
        {
            System.Diagnostics.Process.Start("https://portal.azure.com/#create/Microsoft.CognitiveServices/apitype/Face/pricingtier/S0");
        }

        void _navToCogServicesPage()
        {
            System.Diagnostics.Process.Start("https://www.microsoft.com/cognitive-services/en-us/face-api");
        }


        public string Key
        {
            get { return _confService.FaceApiSubscriptionKey; }
            set
            {
                _confService.FaceApiSubscriptionKey = value;
                OnPropertyChanged();
            }
        }
    }
}
