using System;
using Contracts.Entity;

namespace Contracts.Interfaces
{
    public interface ILogService
    {
        void Log(string text);
        Action<string> Callback { get; set; }
        event EventHandler<TextEventArgs> Logged;
    }
}