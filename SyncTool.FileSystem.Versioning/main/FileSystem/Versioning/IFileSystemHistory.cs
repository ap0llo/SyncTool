// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System.Collections.Generic;

namespace SyncTool.FileSystem.Versioning
{
    public interface IFileSystemHistory
    {
        /// <summary>
        /// The name of this history
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The Id uniquely identifying this history within a group of histories
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the latest snapshot 
        /// </summary>
        /// <returns>Returns a instance of <see cref="IFileSystemSnapshot"/> or null if there are not snapshots</returns>
        IFileSystemSnapshot LatestFileSystemSnapshot { get; } 

        /// <summary>
        /// Gets all the snapshots from the filesystem history
        /// </summary>
        IEnumerable<IFileSystemSnapshot> Snapshots { get; }

        /// <summary>
        /// Creates a new snapshot with the specified filesystem state
        /// </summary>
        /// <param name="fileSystemState">The state of the filesystem to save in the snapshot</param>
        /// <returns></returns>
        IFileSystemSnapshot CreateSnapshot(IDirectory fileSystemState);

        /// <summary>
        /// Gets all the changes from the initial commit up to the specified snapshot
        /// </summary>
        /// <param name="toId">The id of the last snapshot in the range</param>
        /// <param name="pathFilter">If specified limits the changes included in the diff to the supplied paths</param>
        /// <exception cref="SnapshotNotFoundException">Thrown if the specified snapshot could not be found</exception>
        IFileSystemDiff GetChanges(string toId, string[] pathFilter = null);
        
        /// <summary>
        /// Gets all changes between the specified snapshots
        /// </summary>
        /// <param name="fromId">The id of the snapshot marking the start of the range</param>
        /// <param name="toId">The id of the last snapshot in the range</param>
        /// <param name="pathFilter">If specified limits the changes included in the diff to the supplied paths</param>
        /// <exception cref="SnapshotNotFoundException">Thrown if either if the specified snapshots could not be found</exception>
        IFileSystemDiff GetChanges(string fromId, string toId, string[] pathFilter = null);
    }
}