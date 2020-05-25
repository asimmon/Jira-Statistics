using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace JiraStatistics.GuiApp
{
    public class PersistedOptionsService
    {
        private const string SettingsDirectoryName = "JiraStatistics";
        private const string SettingsFileName = "settings.json";

        private readonly string _settingsDirectoryPath;
        private readonly string _settingsFilePath;

        public PersistedOptionsService()
        {
            var localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            this._settingsDirectoryPath = Path.Combine(localAppDataPath, SettingsDirectoryName);
            this._settingsFilePath = Path.Combine(this._settingsDirectoryPath, SettingsFileName);
        }

        public async Task<PersistedJiraOptions> LoadAsync()
        {
            try
            {
                this.EnsureSettingsDirectoryExists();
                var jsonString = await File.ReadAllTextAsync(this._settingsFilePath);
                return JsonConvert.DeserializeObject<PersistedJiraOptions>(jsonString);
            }
            catch (FileNotFoundException ex)
            {
                throw new PersistedOptionsNotFoundException(ex);
            }
            catch (Exception ex)
            {
                throw new OptionsSerializationException(ex);
            }
        }

        public Task SaveAsync(PersistedJiraOptions persistedOptions)
        {
            try
            {
                this.EnsureSettingsDirectoryExists();
                var jsonString = JsonConvert.SerializeObject(persistedOptions);
                return File.WriteAllTextAsync(this._settingsFilePath, jsonString);
            }
            catch (Exception ex)
            {
                throw new OptionsSerializationException(ex);
            }
        }

        private void EnsureSettingsDirectoryExists()
        {
            Directory.CreateDirectory(this._settingsDirectoryPath);
        }
    }
}