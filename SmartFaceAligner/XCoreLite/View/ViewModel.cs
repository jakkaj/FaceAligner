using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using XamlingCore.Portable.View;
using XamlingCore.Portable.View.ViewModel.Base;
using XCoreLite.Contract;

namespace XCoreLite.View
{
    public class ViewModel : NotifyBase, IDisposable
    {
        public IXNavigator Navigator { get; set; }

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

        public ICommand BackCommand => new XCommand(NavigateBack);

        public async Task NavigateTo<T>(Func<T, Task> createdCallback = null)
            where T : ViewModel
        {
            await Navigator.NavigateTo<T>(createdCallback);
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
