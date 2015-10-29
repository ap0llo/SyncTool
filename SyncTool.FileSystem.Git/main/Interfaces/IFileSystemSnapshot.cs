using System;

namespace SyncTool.FileSystem
{
    public interface IFileSystemSnapshot
    {
        string Id { get; }

        DateTime CreationTime { get; } 

        IDirectory RootDirectory { get; }
    }
}