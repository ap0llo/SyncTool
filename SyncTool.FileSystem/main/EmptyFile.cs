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

        public Stream OpenRead()
        {
            return new MemoryStream(Array.Empty<byte>());
        }

      
    }
}