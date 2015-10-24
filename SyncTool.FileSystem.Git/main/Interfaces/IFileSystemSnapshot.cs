using System;

namespace SyncTool.FileSystem
{
    public interface IFileSystemSnapshot
    {
        string Id { get; }

        DateTime CreationTime { get; } 

        Directory RootDirectory { get; }
    }
}