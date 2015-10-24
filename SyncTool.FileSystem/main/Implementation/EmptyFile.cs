using System;
using System.IO;

namespace SyncTool.FileSystem
{
    public class EmptyFile : File, IReadableFile
    {

        public EmptyFile()
        {
            
        }

        public EmptyFile(string name) : base(name)
        {
        }

        public Stream Open(FileMode mode)
        {
            return new MemoryStream(Array.Empty<byte>());
        }

        public override long Length { get; set; }
      
    }
}