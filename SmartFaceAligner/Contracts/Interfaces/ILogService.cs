using System;

namespace Contracts.Interfaces
{
    public interface ILogService
    {
        void Log(string text);
        Action<string> Callback { get; set; }
    }
}