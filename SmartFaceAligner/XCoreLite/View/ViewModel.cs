using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Autofac;
using XamlingCore.Portable.Contract.UI;
using XamlingCore.Portable.View;
using XamlingCore.Portable.View.ViewModel.Base;
using XCoreLite.Contract;

namespace XCoreLite.View
{
    public class ViewModel : NotifyBase, IDisposable
    {
        public IXNavigator Navigator { get; set; }
        public ILifetimeScope Scope { get; set; }

        public IDispatcher Dispatcher { get; set; }

#pragma warning disable 1998
        public virtual async Task Initialised()
#pragma warning restore 1998
        {
            
        }

#pragma warning disable 1998
        public virtual async Task NavigatedTo(bool isBack)
#pragma warning restore 1998
        {

        }

#pragma warning disable 1998
        public virtual async Task NavigatingAway(bool isBack)
#pragma warning restore 1998
        {

        }

        public ICommand BackCommand => new XCommand(NavigateBack);

        public ICommand Command(Action action)
        {
            return new XCommand(action);
        }

        public Task NavigateBackTo<T>()
        {
            return Navigator.NavigateBackTo<T>();
        }

        public Task NavigateTo<T>()
            where T : ViewModel
        {
            return Navigator.NavigateTo<T>();
        }

        public Task NavigateTo<T>(Func<T, Task> createdCallback)
            where T : ViewModel
        {
            return Navigator.NavigateTo<T>(createdCallback);
        }

        public Task NavigateTo<T>(Action<T> createdCallback)
            where T : ViewModel
        {
            return Navigator.NavigateTo<T>(createdCallback);
        }

        public void NavigateBack()
        {
            Navigator.NavigateBack();
        }

        public virtual void Dispose()
        {
        }
    }
}
