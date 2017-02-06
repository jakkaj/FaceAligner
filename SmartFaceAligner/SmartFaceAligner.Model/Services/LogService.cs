﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contracts.Interfaces;

namespace SmartFaceAligner.Processor.Services
{
    public class LogService : ILogService
    {
        public void Log(string text)
        {
            Debug.WriteLine(text);
        }
    }
}
