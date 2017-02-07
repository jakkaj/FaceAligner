using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contracts.Entity;
using Contracts.Interfaces;
using SmartFaceAligner.Processor.Entity;

namespace SmartFaceAligner.Processor.Services
{
    public class LogService : ILogService
    {
        public Action<string> Callback { get; set; }

        public event EventHandler<TextEventArgs> Logged; 

        public void Log(string text)
        {
            Debug.WriteLine(text);

            Callback?.Invoke(text);

            Logged?.Invoke(this, new TextEventArgs(text));
        }

        
    }
}
