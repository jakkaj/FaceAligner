using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartFaceAligner.Processor.Contract;
using SmartFaceAligner.View.Setup;
using XCoreLite.View;

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
