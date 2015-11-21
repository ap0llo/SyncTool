// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using SyncTool.Configuration.Model;
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
                            { 
                                ""name"" : ""foo"", 
                                ""path"" : ""bar""                                 
                            },
                            {   
                                ""name"" : ""foo2"", 
                                ""path"" : ""bar2"",                                 
                                ""readFilter"" : { ""type"" : ""microscopeQuery"" , ""query"" : ""test"" }
                            }
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

            var firstSyncGroup = syncGroups[0];

            Assert.Equal("dir1", firstSyncGroup.Name);
            Assert.Equal("path1", firstSyncGroup.MasterRepositoryPath);
            Assert.Equal("path2", firstSyncGroup.LocalRepositoryPath);
            Assert.Equal(2, firstSyncGroup.Folders.Count());

            Assert.Equal("foo", firstSyncGroup.Folders.First().Name);
            Assert.Equal("bar", firstSyncGroup.Folders.First().Path);
            Assert.Null(firstSyncGroup.Folders.First().ReadFilter);

            Assert.Equal("foo2", firstSyncGroup.Folders.Last().Name);
            Assert.Equal("bar2", firstSyncGroup.Folders.Last().Path);
            Assert.NotNull(firstSyncGroup.Folders.Last().ReadFilter);
            Assert.Equal(FileSystemFilterType.MicroscopeQuery, firstSyncGroup.Folders.Last().ReadFilter.Type);
            Assert.Equal("test", firstSyncGroup.Folders.Last().ReadFilter.Query);

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