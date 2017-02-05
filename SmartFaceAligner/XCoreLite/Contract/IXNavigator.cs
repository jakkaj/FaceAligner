using System;
using System.Threading.Tasks;
using XCoreLite.View;

namespace XCoreLite.Contract
{
    public interface IXNavigator
    {
        Task NavigateBack();

        Task NavigateTo<T>(Func<T, Task> createdCallback = null)
            where T : ViewModel;


        Task NavigateBackTo<T>();
    }
}