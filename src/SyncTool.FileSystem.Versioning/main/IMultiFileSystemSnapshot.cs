using System;
using System.Collections.Generic;
using NodaTime;

namespace SyncTool.FileSystem.Versioning
{
    public interface IMultiFileSystemSnapshot
    {
        string Id { get; }

        /// <summary>
        /// The time the snapshot was created
        /// </summary>
        Instant CreationTime { get; }

        IEnumerable<string> HistoryNames { get; }

        IFileSystemSnapshot GetSnapshot(string historyName);

        string GetSnapshotId(string historyName);

        /// <summary>
        /// Gets the current version of the file specified by the path from the underlying snapshots
        /// as Tuple of history name and file
        /// If a file does not exist, a tuple of (NAME, null) is returns
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        IEnumerable<(string historyName, IFile file)> GetFiles(string path);
    }
}