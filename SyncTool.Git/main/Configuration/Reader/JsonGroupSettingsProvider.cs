// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using SyncTool.Git.Configuration.Model;

namespace SyncTool.Git.Configuration.Reader
{
    public class JsonGroupSettingsProvider : IGroupSettingsProvider
    {
        const string s_SettingsFileName = "SyncTool.Groups.json";


        static string SettingsPath => Path.Combine(Environment.CurrentDirectory, s_SettingsFileName);


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