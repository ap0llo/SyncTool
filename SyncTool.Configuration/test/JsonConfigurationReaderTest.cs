// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using SyncTool.Configuration.Reader;
using Xunit;

namespace SyncTool.Configuration.Test
{
    public class JsonConfigurationReaderTest : IDisposable
    {
        
        readonly JsonConfigurationReader m_Instance = new JsonConfigurationReader();
        readonly string m_SettingsFilePath = Path.Combine(Environment.CurrentDirectory, "SyncTool.settings.json");


        [Fact(DisplayName = nameof(JsonConfigurationReader) + ".GetSyncFolders() throws " + nameof(ConfigurationNotFoundException) + " if settings file does not exist")]
        public void GetSyncFolders_throws_ConfigurationNotFoundException_if_settings_file_does_not_exist()
        {
            Assert.Throws<ConfigurationNotFoundException>(() => m_Instance.GetSyncGroups());            
        }



        [Fact(DisplayName = nameof(JsonConfigurationReader) + ".GetSyncFolders() successfully reads valid json file")]
        public void GetSyncFolders_successfully_reads_valid_json_file()
        {
            
            File.WriteAllText(m_SettingsFilePath,
                @"
                [ 
                    { 
                        ""name"" : ""dir1"" ,
                        ""masterRepositoryPath"" : ""path1"",
                        ""localRepositoryPath"" : ""path2"",
                        ""folders"" : 
                        [
                            { ""name"" : ""foo"", ""path"" : ""bar"" }
                        ]
                    },
                    { 
                        ""name"" : ""dir2"" ,
                        ""masterRepositoryPath"" : ""path3"",
                        ""localRepositoryPath"" : ""path4""
                    }
                ]
                ");

            var syncGroups = m_Instance.GetSyncGroups().ToList();

            Assert.Equal(2, syncGroups.Count);

            Assert.Equal("dir1", syncGroups[0].Name);
            Assert.Equal("path1", syncGroups[0].MasterRepositoryPath);
            Assert.Equal("path2", syncGroups[0].LocalRepositoryPath);
            Assert.Single(syncGroups[0].Folders);
            Assert.Equal("foo", syncGroups[0].Folders.Single().Name);
            Assert.Equal("bar", syncGroups[0].Folders.Single().Path);

            Assert.Equal("dir2", syncGroups[1].Name);
            Assert.Equal("path3", syncGroups[1].MasterRepositoryPath);
            Assert.Equal("path4", syncGroups[1].LocalRepositoryPath);
            Assert.Empty(syncGroups[1].Folders);

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