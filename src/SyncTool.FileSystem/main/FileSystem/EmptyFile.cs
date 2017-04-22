using System;
using System.IO;

namespace SyncTool.FileSystem
{
    public class EmptyFile : File, IReadableFile
    {

        public EmptyFile(string name) : this(null, name)
        {
            
        }

        public EmptyFile(IDirectory parent, string name) : base(parent, name)
        {
        }

        public Stream OpenRead()
        {
            return new MemoryStream(Array.Empty<byte>());
        }


        public override IFile WithParent(IDirectory newParent)
        {
            return new EmptyFile(newParent, this.Name) { LastWriteTime =  this.LastWriteTime, Length =  this.Length};
        }
    }
}