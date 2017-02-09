using System;
using Contracts.Entity;

namespace Contracts.Interfaces
{
    public interface ILogService
    {
        void Log(string text, bool debug = true);
        Action<string> Callback { get; set; }
        event EventHandler<TextEventArgs> Logged;
        void LogException(Exception ex);
    }
}