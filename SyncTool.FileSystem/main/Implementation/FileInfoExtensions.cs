using System.IO;
using System.Security.AccessControl;

namespace SyncTool.FileSystem
{
    public static class FileInfoExtensions
    {
        public static File ToFile(this FileInfo fileInfo) => new File() {LastWriteTime = fileInfo.LastWriteTime, Length = fileInfo.Length, Name = fileInfo.Name};

        public static Directory ToDirectory(this DirectoryInfo directoryInfo) => new Directory() { Name = directoryInfo.Name};
    }
}