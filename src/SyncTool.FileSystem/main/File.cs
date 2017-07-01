using System;

namespace SyncTool.FileSystem
{
    public class File : FileSystemItem, IFile
    {
        public DateTime LastWriteTime { get; set; }

        public long Length { get; set; }


        public File(IDirectory parent , string name) : base(parent, name)
        {
        }

        public virtual IFile WithParent(IDirectory newParent) 
            => new File(newParent, Name)
               {
                   LastWriteTime = LastWriteTime,
                   Length = this.Length
               };
    }
}