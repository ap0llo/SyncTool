using System;

namespace SyncTool.FileSystem
{
    public static class FileReferenceExtensions
    {
        public static bool Matches(this IFileReference reference, IFile file)
        {
            if (reference == null)
            {
                throw new ArgumentNullException(nameof(reference));
            }

            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            return StringComparer.InvariantCultureIgnoreCase.Equals(reference.Path, file.Path) &&
                   (!reference.LastWriteTime.HasValue || reference.LastWriteTime == file.LastWriteTime) &&
                   (!reference.Length.HasValue || reference.Length == file.Length);
        }
    }
}