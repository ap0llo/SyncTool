using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Xunit;

namespace SyncTool.FileSystem.Git.Test
{
    public class DirectoryPropertiesFileTest
    {
        const string s_Dir1 = "dir1";
        readonly JsonSerializer m_Serializer = new JsonSerializer();



        [Fact]
        public void Open_returns_json_readable_stream()
        {
            var directory = new Directory(s_Dir1);
            var directoryPropertiesFile = DirectoryPropertiesFile.ForDirectory(directory);

            DirectoryProperties properties;
            using (var jsonReader = new JsonTextReader(new StreamReader(directoryPropertiesFile.Open(FileMode.Open))))
            {
                properties = m_Serializer.Deserialize<DirectoryProperties>(jsonReader);
            }

            Assert.NotNull(properties);
            Assert.Equal(directory.Name, properties.Name);

        }


        [Fact]
        public void Open_throws_NotSupportedException_for_FileMode_other_than_open()
        {
            var directoryPropertiesFile = DirectoryPropertiesFile.ForDirectory(new Directory(s_Dir1));
            foreach (var mode in Enum.GetValues(typeof(FileMode)).Cast<FileMode>().Where(m => m != FileMode.Open))
            {
                Assert.Throws<NotSupportedException>(() => directoryPropertiesFile.Open(mode));
            }

        }
    }
}