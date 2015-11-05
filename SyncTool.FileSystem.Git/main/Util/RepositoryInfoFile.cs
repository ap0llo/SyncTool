using System;
using System.IO;

namespace SyncTool.FileSystem.Git
{
    public class RepositoryInfoFile : IReadableFile
    {
        public const string RepositoryInfoFileName = "SyncToolRepositoryInfo.json";


        public string Name => RepositoryInfoFileName;

        public DateTime LastWriteTime { get; }

        public long Length
        {
            get { throw new NotImplementedException(); }
        }

        public RepositoryInfo Content { get; }



        public RepositoryInfoFile()
        {
            LastWriteTime = DateTime.Now;
            Content = new RepositoryInfo();
        }



        public Stream OpenRead()
        {
            using (var writeStream = new MemoryStream())
            {
                Content.WriteTo(writeStream);
                writeStream.Flush();

                return new MemoryStream(writeStream.ToArray());
            }
        }
    }
}