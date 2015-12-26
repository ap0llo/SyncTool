// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Xunit;

namespace SyncTool.FileSystem.Versioning.Git.MetaFileSystem
{
    /// <summary>
    /// Tests for <see cref="FilePropertiesFile"/>
    /// </summary>
    public class FilePropertiesFileTest
    {
        readonly JsonSerializer m_Serializer = new JsonSerializer();

        [Fact(DisplayName = "FilePropertiesFile.Open() returns json readable stream")]
        public void Open_returns_json_readable_stream()
        {
            var file = new EmptyFile("file1") { LastWriteTime = DateTime.Now, Length = 42};
            var filePropertiesFile = FilePropertiesFile.ForFile(null, file);

            FileProperties properties;
            using (var jsonReader = new JsonTextReader(new StreamReader(filePropertiesFile.OpenRead())))
            {
                properties = m_Serializer.Deserialize<FileProperties>(jsonReader);
            }

            Assert.NotNull(properties);
            Assert.Equal(file.Name, properties.Name);
            Assert.Equal(file.Length, properties.Length);
            Assert.Equal(file.LastWriteTime, properties.LastWriteTime);
        }        

    }
}