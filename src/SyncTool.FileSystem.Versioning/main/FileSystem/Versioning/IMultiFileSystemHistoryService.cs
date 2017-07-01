using System;
using System.Collections.Generic;
using SyncTool.Common.Services;

namespace SyncTool.FileSystem.Versioning
{
    public interface IMultiFileSystemHistoryService : IService
    {         
        /// <summary>
        /// Gets the lastest snapshot or null, if no snapshot was created yet
        /// </summary>
        IMultiFileSystemSnapshot LatestSnapshot { get; }
    
        /// <summary>
        /// Enumerates all snapshots
        /// </summary>
        IEnumerable<IMultiFileSystemSnapshot> Snapshots { get; }

        /// <summary>
        /// Gets the snapshot with the specified id
        /// </summary>
        /// <param name="id">The id of the snapshot to retrieve</param>
        /// <exception cref="ArgumentNullException">Thrown if the specified id is null, empty or whitespace</exception>
        /// <exception cref="SnapshotNotFoundException">Thrown if the specified snapshot was not found</exception>
        IMultiFileSystemSnapshot this[string id] { get; }

        /// <summary>
        /// Saves the latest snapshots of the underlying file system histories
        /// </summary>
        IMultiFileSystemSnapshot CreateSnapshot();

        /// <summary>
        /// Gets the paths of all changed files from the initial snapshot up to the specified snapshot
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if the specified id is null, empty or whitespace</exception>
        /// <exception cref="SnapshotNotFoundException">Thrown if the specified snapshot was not found</exception>
        string[] GetChangedFiles(string toId);

        /// <summary>
        /// Gets the paths of all changed files in the specified range
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if one of the specified ids is null, empty or whitespace</exception>
        /// <exception cref="SnapshotNotFoundException">Thrown if on of the specified snapshots was not found</exception>
        /// <exception cref="InvalidRangeException">
        /// Thrown if the specified range is invalid. The snapshot referenced by <param name="fromId"/> 
        /// must be an ancestor of the snapshot referenced by <param name="toId" />
        /// </exception>
        string[] GetChangedFiles(string fromId, string toId);

        /// <summary>
        /// Gets the changes between the initial and the specified snapshot
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if the specified id is null, empty or whitespace</exception>
        /// <exception cref="SnapshotNotFoundException">Thrown if the specified snapshot was not found</exception>
        /// <exception cref="InvalidRangeException">
        /// Thrown if the specified range is invalid. The snapshot referenced by <param name="fromId"/> 
        /// must be an ancestor of the snapshot referenced by <param name="toId" />
        /// </exception>
        IMultiFileSystemDiff GetChanges(string toId, string[] pathFilter = null);

        /// <summary>
        /// Gets the changes in the specified range
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if one of the specified ids is null, empty or whitespace</exception>
        /// <exception cref="SnapshotNotFoundException">Thrown if on of the specified snapshots was not found</exception>
        IMultiFileSystemDiff GetChanges(string fromId, string toId, string[] pathFilter = null);
    }
}