// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using SyncTool.FileSystem.Git;
using Xunit;

namespace SyncTool.FileSystem.Versioning.Git
{
    public class DirectoryPropertiesFileTest
    {
        const string s_Dir1 = "dir1";
        readonly JsonSerializer m_Serializer = new JsonSerializer();



        [Fact(DisplayName = "DirectoryPropertiesFile.Open() returns json readable stream")]
        public void Open_returns_json_readable_stream()
        {
            var directory = new Directory(s_Dir1);
            var directoryPropertiesFile = DirectoryPropertiesFile.ForDirectory(null, directory);

            DirectoryProperties properties;
            using (var jsonReader = new JsonTextReader(new StreamReader(directoryPropertiesFile.OpenRead())))
            {
                properties = m_Serializer.Deserialize<DirectoryProperties>(jsonReader);
            }

            Assert.NotNull(properties);
            Assert.Equal(directory.Name, properties.Name);
        }

    }
}