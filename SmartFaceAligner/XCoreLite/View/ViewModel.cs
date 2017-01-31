using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XamlingCore.Portable.View.ViewModel.Base;
using XCoreLite.Contract;

namespace XCoreLite.View
{
    public class ViewModel : NotifyBase, IDisposable
    {
        public IXNavigator Navigator { get; set; }

        public async Task NavigateTo<T>(Func<T, Task> createdCallback = null)
            where T : ViewModel
        {
            await Navigator.NavigateTo<T>(createdCallback);
        }

        public void NavigateTo<T>(T viewModel)
            where T : ViewModel
        {
            Navigator.NavigateTo(viewModel);
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
