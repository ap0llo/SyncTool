using System;
using NodaTime;

namespace SyncTool.FileSystem
{
    public sealed class FileReference : IEquatable<FileReference>
    {
        public string Path { get; }

        public Instant? LastWriteTime { get; }

        public long? Length { get; }


        public FileReference(string path, Instant? lastWriteTime = null, long? length = null)
        {
            PathValidator.EnsureIsValidFilePath(path);
            PathValidator.EnsureIsRootedPath(path);

            Path = path;
            LastWriteTime = lastWriteTime;
            Length = length;
        }


        public override int GetHashCode() => StringComparer.InvariantCultureIgnoreCase.GetHashCode(Path);

        public override bool Equals(object obj) => Equals(obj as FileReference);

        public bool Equals(FileReference other)
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