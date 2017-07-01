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

        public Stream OpenRead() => new MemoryStream(Array.Empty<byte>());

        public override IFile WithParent(IDirectory newParent)
            => new EmptyFile(newParent, this.Name)
               {
                   LastWriteTime =  LastWriteTime,
                   Length =  Length
               };
    }
}