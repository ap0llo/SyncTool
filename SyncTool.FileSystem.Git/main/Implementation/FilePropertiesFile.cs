using System;
using System.IO;
using Newtonsoft.Json;

namespace SyncTool.FileSystem.Git
{
    class FilePropertiesFile : IReadableFile
    {
        public const string FileNameSuffix = ".SyncTool.json";

        readonly FileProperties m_Properties;
        readonly JsonSerializer m_Serializer = new JsonSerializer();

        public FilePropertiesFile(IFile file)
        {
            m_Properties = new FileProperties(file);
            Name = file.Name + FileNameSuffix;
            LastWriteTime = DateTime.Now;
        }
        
                
        public string Name { get;  }

        public DateTime LastWriteTime { get; }

        public long Length { get { throw new NotSupportedException();} }

        public Stream Open(FileMode mode)
        {
            if (mode != FileMode.Open)
            {
                throw new NotSupportedException($"{nameof(FilePropertiesFile)} Open() only supports reading");
            }

            var writeStream = new MemoryStream();
            using (var streamWriter = new StreamWriter(writeStream))            
            {
                m_Serializer.Serialize(streamWriter, m_Properties);
                streamWriter.Flush();

                return new MemoryStream(writeStream.ToArray());
            }            
        }
    }
}