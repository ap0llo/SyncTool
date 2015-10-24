using System;

namespace SyncTool.FileSystem
{
    public interface IFile : IFileSystemItem
    {
        DateTime LastWriteTime { get; }

        long Length { get; }
        

    }
}