using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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

      
      
        public async Task NavigateBack()
        {
            if (_navigationService.Content is FrameworkElement element)
            {
                if (element.DataContext is ViewModel vm)
                {
                    await _navigatingAway(_navigationService.Content, true);
                    vm.Dispose();
                }
            }

            if (_navigationService.CanGoBack)
            {
                _navigationService.GoBack();

                if (_navigationService.Content is ViewModel vm)
                {
                    await vm.NavigatedTo(true);
                }
            }
        }

        async Task _navigatingAway(object content, bool isBack)
        {
            if (content is FrameworkElement element)
            {
                if (element.DataContext is ViewModel vm)
                {
                    await vm.NavigatingAway(true);
                }
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

            await newViewModel.Initialised();

            await NavigateTo(newViewModel);
        }

        protected async Task NavigateTo<T>(T viewModel)
            where T: ViewModel
        {
            //find the view model's associated view
            var view = _viewResolver.Resolve(viewModel);

            if (view == null)
            {
                throw new Exception("Could not find v");
            }

            await _navigatingAway(_navigationService.Content, false);

            _navigationService.Navigate(view);

            await viewModel.NavigatedTo(false);
        }
    }
}
