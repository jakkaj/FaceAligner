using System.Threading.Tasks;
using SmartFaceAligner.View.Setup;
using XCoreLite.View;
using Contracts;
using Contracts.Interfaces;

namespace SmartFaceAligner.View
{
    public class HomeViewModel : ViewModel
    {
        private readonly IConfigurationService _configService;

        public HomeViewModel(IConfigurationService configService)
        {
            _configService = configService;
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
