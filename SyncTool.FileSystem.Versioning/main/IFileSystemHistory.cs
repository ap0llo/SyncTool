using System.Collections.Generic;

namespace SyncTool.FileSystem.Versioning
{
    public interface IFileSystemHistory
    {

        string Id { get; }

        IFileSystemSnapshot LatestFileSystemSnapshot { get; } 

        IEnumerable<IFileSystemSnapshot> Snapshots { get; }

        IFileSystemSnapshot CreateSnapshot(Directory fileSystemState);

        IFileSystemDiff CompareSnapshots(string fromId, string toId);
    }
}