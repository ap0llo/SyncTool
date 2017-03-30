// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace SyncTool.Git.Configuration.Reader
{
    /// <summary>
    /// Tests for <see cref="JsonGroupSettingsProvider"/>
    /// </summary>
    public sealed class JsonGroupSettingsProviderTest : IDisposable
    {        
        readonly JsonGroupSettingsProvider m_Instance = new JsonGroupSettingsProvider();
        readonly string m_SettingsFilePath = Path.Combine(Environment.CurrentDirectory, "SyncTool.Groups.json");



        [Fact(DisplayName = nameof(JsonGroupSettingsProvider) + ".GetGroupSettings() successfully reads valid json file")]
        public void GetGroupSettings_successfully_reads_valid_json_file()
        {
            
            File.WriteAllText(m_SettingsFilePath,
                @"
                [ 
                    { 
                        ""name"" : ""name1"",
                        ""address"" : ""address1"",       
                    },
                    {                         
                        ""name"" : ""name2"",
                        ""address"" : ""address2""
                    }
                ]
                ");

            var groupSettings = m_Instance.GetGroupSettings().ToList();

            Assert.Equal(2, groupSettings.Count);

            var firstSyncGroup = groupSettings[0];
            
            Assert.Equal("name1", firstSyncGroup.Name);
            Assert.Equal("address1", firstSyncGroup.Address);
            
                     
            Assert.Equal("name2", groupSettings[1].Name);
            Assert.Equal("address2", groupSettings[1].Address);            

        }

        public void Dispose()
        {
            if (File.Exists(m_SettingsFilePath))
            {
                File.Delete(m_SettingsFilePath);
            }
        }
    }
}