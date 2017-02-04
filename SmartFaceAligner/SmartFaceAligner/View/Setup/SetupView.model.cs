using Contracts;
using System.Windows.Input;
using Contracts.Interfaces;
using XCoreLite.View;

namespace SmartFaceAligner.View.Setup
{
    public class SetupViewModel : ViewModel
    {
        private readonly IConfigurationService _confService;

        public ICommand AzureLinkCommand => Command(_navToAzureSignup);
        public ICommand CogServicesLinkCommand => Command(_navToCogServicesPage);

        public ICommand BingAzureLinkCommand => Command(_navToAzureBing);

        public ICommand BingCogLinkCommand => Command(_navToCogServicesBing);

        public SetupViewModel(IConfigurationService confService)
        {
            _confService = confService;
        }

        void _navToAzureBing()
        {
            System.Diagnostics.Process.Start("https://ms.portal.azure.com/#create/Microsoft.CognitiveServices");
        }

        void _navToCogServicesBing()
        {
            System.Diagnostics.Process.Start("https://www.microsoft.com/cognitive-services/en-us/bing-image-search-api");
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

        public string BingKey
        {
            get { return _confService.BingSearchSubscriptionKey; }
            set
            {
                _confService.BingSearchSubscriptionKey = value;
                OnPropertyChanged();
            }
        }
    }
}
