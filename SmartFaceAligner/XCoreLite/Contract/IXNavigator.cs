using System;
using System.Threading.Tasks;
using XCoreLite.View;

namespace XCoreLite.Contract
{
    public interface IXNavigator
    {
        void NavigateBack();

        Task NavigateTo<T>(Func<T, Task> createdCallback = null)
            where T : ViewModel;

        void NavigateTo<T>(T viewModel)
            where T: ViewModel;
    }
}