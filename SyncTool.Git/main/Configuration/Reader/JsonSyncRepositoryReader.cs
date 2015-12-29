// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using SyncTool.Configuration;
using SyncTool.Git.Configuration.Model;

namespace SyncTool.Git.Configuration.Reader
{
    public class JsonSyncRepositoryReader : ISyncRepositoryReader
    {
        const string s_SettingsFileName = "SyncTool.Repositories.json";




        public IEnumerable<SyncRepositorySettings> GetSyncRepositories()
        {
            var settingsFilePath = Path.Combine(Environment.CurrentDirectory, s_SettingsFileName);

            if(!File.Exists(settingsFilePath))
            {
                throw new ConfigurationNotFoundException($"Configuration file '{s_SettingsFileName}' not found");
            }

            return JsonConvert.DeserializeObject<SyncRepositorySettings[]>(File.ReadAllText(settingsFilePath));            
        }
    }
}