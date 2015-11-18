// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using SyncTool.Configuration.Model;

namespace SyncTool.Configuration.Reader
{
    public class JsonConfigurationReader : IConfigurationReader
    {
        const string s_SettingsFileName = "SyncTool.settings.json";




        public IEnumerable<SyncGroup> GetSyncGroups()
        {
            var settingsFilePath = Path.Combine(Environment.CurrentDirectory, s_SettingsFileName);

            if(!File.Exists(settingsFilePath))
            {
                throw new ConfigurationNotFoundException($"Configuration file '{s_SettingsFileName}' not found");
            }

            return JsonConvert.DeserializeObject<SyncGroup[]>(File.ReadAllText(settingsFilePath));            
        }
    }
}