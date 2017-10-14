using System.Collections.Generic;

namespace SyncTool.FileSystem.Versioning
{
    public interface IMultiFileSystemDiff
    {
        IMultiFileSystemSnapshot FromSnapshot { get; } 

        IMultiFileSystemSnapshot ToSnapshot { get; }

        /// <summary>
        /// Gets a combined list of all file changes between the two snapshots
        /// </summary>
        IEnumerable<MultiFileSystemChangeList> FileChanges { get; }

        /// <summary>
        /// Gets a list of changes of individual histories
        /// </summary>
        IEnumerable<HistoryChange> HistoryChanges { get; }
    }
}