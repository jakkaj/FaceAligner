using System;
using System.Threading.Tasks;
using XCoreLite.View;

namespace XCoreLite.Contract
{
    public interface IXNavigator
    {
        Task NavigateBack();

        Task NavigateTo<T>(Func<T, Task> createdCallback)
            where T : ViewModel;

        Task NavigateTo<T>(Action<T> createdCallback)
            where T : ViewModel;


        Task NavigateBackTo<T>();

        Task NavigateTo<T>()
            where T : ViewModel;
    }
}