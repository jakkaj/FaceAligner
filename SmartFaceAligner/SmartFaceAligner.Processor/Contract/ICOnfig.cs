using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XamlingCore.Portable.Contract.Config;

namespace SmartFaceAligner.Processor.Contract
{
    public interface IConfigurationService : IConfig
    {
        string GetSavedSetting(string settingName);
        void SaveSetting(string settingName, string settingValue);
        bool NeedsConfig { get; }
        string FaceApiSubscriptionKey { get; }
    }
}
