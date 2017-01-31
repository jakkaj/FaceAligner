using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using Autofac;
using XCoreLite.Contract;
using XCoreLite.View;

namespace XCoreLite.Navigation
{
    public class XNavigator : IXNavigator
    {
        private readonly ILifetimeScope _scope;
        private readonly IWindowsNativeViewResolver _viewResolver;
        private readonly NavigationService _navigationService;

        public XNavigator(ILifetimeScope scope, 
            IWindowsNativeViewResolver viewResolver,
            NavigationService navigationService)
        {
            _scope = scope;
            _viewResolver = viewResolver;
            _navigationService = navigationService;
        }

        public void NavigateBack()
        {
            if(_navigationService.CurrentSource is IDisposable dispose)
            {
                dispose.Dispose();
            }

            if (_navigationService.CanGoBack)
            {
                _navigationService.GoBack();
            }
        }

        public async Task NavigateTo<T>(Func<T, Task> createdCallback = null)
            where T : ViewModel
        {
            var newViewModel = _scope.Resolve<T>();

            if (newViewModel == null)
            {
                throw new Exception("Could not find vm");
            }

            if (createdCallback != null)
            {
                await createdCallback(newViewModel);
            }

            NavigateTo(newViewModel);
        }

        public void NavigateTo<T>(T viewModel)
            where T: ViewModel
        {
            //find the view model's associated view
            var view = _viewResolver.Resolve(viewModel);

            if (view == null)
            {
                throw new Exception("Could not find v");
            }

            _navigationService.Navigate(view);
        }
    }
}
