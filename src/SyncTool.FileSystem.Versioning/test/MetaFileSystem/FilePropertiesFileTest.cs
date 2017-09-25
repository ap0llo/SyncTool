using System.IO;
using Newtonsoft.Json;
using Xunit;
using SyncTool.FileSystem.Versioning.MetaFileSystem;
using NodaTime;
using NodaTime.Serialization.JsonNet;

namespace SyncTool.FileSystem.Versioning.Test.MetaFileSystem
{
    /// <summary>
    /// Tests for <see cref="FilePropertiesFile"/>
    /// </summary>
    public class FilePropertiesFileTest
    {
        readonly JsonSerializer m_Serializer = new JsonSerializer().ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

        [Fact]
        public void Open_returns_json_readable_stream()
        {
            var file = new EmptyFile("file1") { LastWriteTime = SystemClock.Instance.GetCurrentInstant(), Length = 42};
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