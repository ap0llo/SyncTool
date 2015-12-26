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

namespace SyncTool.Configuration.Git.Reader
{
    public sealed class JsonSyncRepositoryReaderTest : IDisposable
    {        
        readonly JsonSyncRepositoryReader m_Instance = new JsonSyncRepositoryReader();
        readonly string m_SettingsFilePath = Path.Combine(Environment.CurrentDirectory, "SyncTool.Repositories.json");


        [Fact(DisplayName = nameof(JsonSyncRepositoryReader) + ".GetSyncRepositories() throws " + nameof(ConfigurationNotFoundException) + " if settings file does not exist")]
        public void GetSyncRepositories_throws_ConfigurationNotFoundException_if_settings_file_does_not_exist()
        {
            Assert.Throws<ConfigurationNotFoundException>(() => m_Instance.GetSyncRepositories());            
        }



        [Fact(DisplayName = nameof(JsonSyncRepositoryReader) + ".GetSyncFolders() successfully reads valid json file")]
        public void GetSyncFolders_successfully_reads_valid_json_file()
        {
            
            File.WriteAllText(m_SettingsFilePath,
                @"
                [ 
                    { 
                        ""masterRepositoryPath"" : ""path1"",
                        ""localRepositoryPath"" : ""path2"",       
                    },
                    {                         
                        ""masterRepositoryPath"" : ""path3"",
                        ""localRepositoryPath"" : ""path4""
                    }
                ]
                ");

            var repositories = m_Instance.GetSyncRepositories().ToList();

            Assert.Equal(2, repositories.Count);

            var firstSyncGroup = repositories[0];
            
            Assert.Equal("path1", firstSyncGroup.MasterRepositoryPath);
            Assert.Equal("path2", firstSyncGroup.LocalRepositoryPath);
            
                     
            Assert.Equal("path3", repositories[1].MasterRepositoryPath);
            Assert.Equal("path4", repositories[1].LocalRepositoryPath);            

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