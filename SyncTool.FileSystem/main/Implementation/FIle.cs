using System;
using static System.IO.Path;

namespace SyncTool.FileSystem
{
    public class File : FileSystemItem, IEquatable<File>
    {
        public DateTime LastWriteTime { get; set; }

        public long Length { get; set; }



        public override int GetHashCode() => StringComparer.InvariantCultureIgnoreCase.GetHashCode(Path);

        public override bool Equals(object obj) => Equals(obj as File);
        
        public bool Equals(File other)
        {
            if (other == null)
            {
                return false;
            }

            return StringComparer.InvariantCultureIgnoreCase.Equals(this.Path, other.Path);
        }


    }
}