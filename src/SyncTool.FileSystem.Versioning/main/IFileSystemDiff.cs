using System.Collections.Generic;

namespace SyncTool.FileSystem.Versioning
{
    public interface IFileSystemDiff
    {
        /// <summary>
        /// Gets the <see cref="IFileSystemHistory"/> the diff was taken from
        /// </summary>
        IFileSystemHistory History { get; }

        /// <summary>
        /// The snapshot that serves as base of the diff        
        /// </summary>
        /// <remarks>Might be null when the diff describes all changes ever made to a file system</remarks>
        IFileSystemSnapshot FromSnapshot { get; }

        /// <summary>
        /// The latest snapshot that was included in the diff
        /// </summary>
        IFileSystemSnapshot ToSnapshot { get; }

        /// <summary>
        /// Gets a change list for every file that changed between the two snapshots
        /// </summary>
        IEnumerable<ChangeList> ChangeLists { get; } 
    }
}