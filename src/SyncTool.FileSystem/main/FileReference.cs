using System;

namespace SyncTool.FileSystem
{
    public sealed class FileReference : IFileReference
    {
        public string Path { get; }

        public DateTime? LastWriteTime { get; }

        public long? Length { get; }


        public FileReference(string path, DateTime? lastWriteTime = null, long? length = null)
        {
            PathValidator.EnsureIsValidFilePath(path);
            PathValidator.EnsureIsRootedPath(path);

            Path = path;
            LastWriteTime = lastWriteTime;
            Length = length;
        }


        public override int GetHashCode() => StringComparer.InvariantCultureIgnoreCase.GetHashCode(Path);

        public override bool Equals(object obj) => Equals(obj as IFileReference);

        public bool Equals(IFileReference other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return StringComparer.InvariantCultureIgnoreCase.Equals(this.Path, other.Path) &&
                   LastWriteTime == other.LastWriteTime &&
                   Length == other.Length;            
        }
    }
}