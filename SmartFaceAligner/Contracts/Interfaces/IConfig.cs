using XamlingCore.Portable.Contract.Config;

namespace Contracts.Interfaces
{
    public interface IConfigurationService : IConfig
    {
        string GetSavedSetting(string settingName);
        void SaveSetting(string settingName, string settingValue);
        bool NeedsConfig { get; }
        string FaceApiSubscriptionKey { get; set; }
        string BingSearchSubscriptionKey { get; set; }
    }
}
