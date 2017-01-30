using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SmartFaceAligner.Processor.Contract;
using SmartFaceAligner.Processor.Entity;
using XamlingCore.Portable.Contract.Config;

namespace SmartFaceAligner.Processor.Services
{
    public class ConfigService : IConfigurationService
    {
        private readonly List<Settings> _settings;

        public ConfigService()
        {
            _settings = _getSettings();
        }

        public bool NeedsConfig => GetSavedSetting(ProcessorContstants.Settings.SubsKeys) == null;
        public string FaceApiSubscriptionKey => GetSavedSetting(ProcessorContstants.Settings.SubsKeys);

        #region SaveSettings


        public string GetSavedSetting(string settingName)
        {
            return _settings.FirstOrDefault(_ => _.Name == settingName)?.Value;
        }

        public void SaveSetting(string settingName, string settingValue)
        {
            var existing = _settings.FirstOrDefault(_ => _.Name == settingName);

            if (existing == null)
            {
                existing = new Settings
                {
                    Name = settingName
                };

                _settings.Add(existing);
            }

            existing.Value = settingValue;
            _saveSettings(_settings);

        }

        List<Settings> _getSettings()
        {
            var fn = _getSettingsFileName();

            if (!File.Exists(fn))
            {
                return new List<Settings>();
            }

            var data = File.ReadAllText(fn);

            return JsonConvert.DeserializeObject<List<Settings>>(data);
        }

        void _saveSettings(List<Settings> settings)
        {
            var data = JsonConvert.SerializeObject(settings);

            File.WriteAllText(_getSettingsFileName(), data);
        }

        string _getSettingsFileName()
        {
            var settingsFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), $"SmartFaceAligner\\settings.json");
            return settingsFile;
        }

        #endregion


        public string this[string index] => ConfigurationManager.AppSettings[index];

    }
}
