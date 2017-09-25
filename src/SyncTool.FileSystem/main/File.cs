using NodaTime;

namespace SyncTool.FileSystem
{
    public class File : FileSystemItem, IFile
    {
        public Instant LastWriteTime { get; set; } = Instant.MinValue;

        public long Length { get; set; }


        public File(IDirectory parent , string name) : base(parent, name)
        {
        }


        public virtual IFile WithParent(IDirectory newParent) 
            => new File(newParent, Name)
               {
                   LastWriteTime = LastWriteTime,
                   Length = Length
               };
    }
}