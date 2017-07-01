using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using SyncTool.Common;

namespace SyncTool.Common.Groups
{
    public class JsonGroupSettingsProvider : IGroupSettingsProvider
    {
        const string s_SettingsFileName = "SyncTool.Groups.json";

        readonly string m_Directory;


        string SettingsPath => Path.Combine(m_Directory, s_SettingsFileName);

        public JsonGroupSettingsProvider() : this(Environment.CurrentDirectory)
        {

        }

        public JsonGroupSettingsProvider(string directory)
        {
            if (directory == null)
                throw new ArgumentNullException(nameof(directory));

            if (!Directory.Exists(directory))
                throw new DirectoryNotFoundException($"Directory '{directory}' does not exist");

            m_Directory = directory;
        }

        

        public IEnumerable<GroupSettings> GetGroupSettings()
        {            
            if (!File.Exists(SettingsPath))
            {
                return Enumerable.Empty<GroupSettings>();
            }

            return JsonConvert.DeserializeObject<GroupSettings[]>(File.ReadAllText(SettingsPath));
        }
        

        public void SaveGroupSettings(IEnumerable<GroupSettings> settings)
        {
            var json = JsonConvert.SerializeObject(settings.ToArray(), Formatting.Indented);
            File.WriteAllText(SettingsPath, json);
        }


        
    }
}