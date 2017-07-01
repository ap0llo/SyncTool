using System.Collections.Generic;

namespace SyncTool.FileSystem
{
    public static class DirectoryExtensions
    {
        /// <summary>
        /// Gets the file with the specified path from the directory or null if the file does not exist
        /// </summary>
        public static IFile GetFileOrDefault(this IDirectory directory, string path) 
            => directory.FileExists(path) ? directory.GetFile(path) : default(IFile);


        public static IEnumerable<IFile> EnumerateFilesRecursively(this IDirectory directory)
        {
            foreach (var file in directory.Files)
            {
                yield return file;
            }

            foreach (var dir in directory.Directories)
            {
                foreach (var file in dir.EnumerateFilesRecursively())
                {
                    yield return file;
                }
            }
        }
    }
}