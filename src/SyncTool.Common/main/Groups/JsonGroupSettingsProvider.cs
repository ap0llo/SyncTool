using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using SyncTool.Common.Options;

namespace SyncTool.Common.Groups
{
    sealed class JsonGroupSettingsProvider : IGroupSettingsProvider
    {
        const string s_SettingsFileName = "SyncTool.Groups.json";

        readonly string m_Directory;


        string SettingsPath => Path.Combine(m_Directory, s_SettingsFileName);


        public JsonGroupSettingsProvider(ApplicationDataOptions options) : this(options.RootPath)
        {
        }

        public JsonGroupSettingsProvider(string directory)
        {
            m_Directory = directory ?? throw new ArgumentNullException(nameof(directory));
        }


        public IEnumerable<GroupSettings> GetGroupSettings() => 
            File.Exists(SettingsPath)
                ? JsonConvert.DeserializeObject<GroupSettings[]>(File.ReadAllText(SettingsPath))
                : Enumerable.Empty<GroupSettings>();

        public void SaveGroupSettings(IEnumerable<GroupSettings> settings)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath));
            File.WriteAllText(SettingsPath, JsonConvert.SerializeObject(settings.ToArray(), Formatting.Indented));
        }        
    }
}