using System;
using System.Windows.Threading;
using XamlingCore.Portable.Contract.UI;
namespace SmartFaceAligner.Util
{
    public class XDispatcher : IDispatcher
    {
        public static Dispatcher Dispatcher { get; set; }
        public void Invoke(Action action)
        {
            Dispatcher.Invoke(action);
        }
    }
    }
}
