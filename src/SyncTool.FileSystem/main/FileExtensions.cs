using System;
using System.IO;

namespace SyncTool.FileSystem
{
    /// <summary>
    /// Extension methods for <see cref="IFile"/>
    /// </summary>
    public static class FileExtensions
    {
        static readonly char[] s_TrimChars = " .".ToCharArray();


        public static string GetExtension(this IFile file) => Path.GetExtension(file.Name);

        public static bool HasExtension(this IFile file, string extension)
        {
            return file.GetExtension().TrimStart(s_TrimChars).Equals(extension.TrimStart(s_TrimChars), StringComparison.InvariantCultureIgnoreCase);
        }

        public static IFileReference ToReference(this IFile file) => new FileReference(file.Path, file.LastWriteTime, file.Length);
    }
}