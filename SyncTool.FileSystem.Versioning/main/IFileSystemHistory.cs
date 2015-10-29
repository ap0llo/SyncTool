using System.Collections.Generic;

namespace SyncTool.FileSystem
{
    public interface IFileSystemHistory
    {

        string Id { get; }

        IFileSystemSnapshot LatestFileSystemSnapshot { get; } 

        IEnumerable<IFileSystemSnapshot> Snapshots { get; }

        IFileSystemSnapshot CreateSnapshot(Directory fileSystemState);
    }
}