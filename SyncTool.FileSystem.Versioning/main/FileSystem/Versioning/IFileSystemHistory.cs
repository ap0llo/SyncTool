// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System.Collections.Generic;

namespace SyncTool.FileSystem.Versioning
{
    public interface IFileSystemHistory
    {

        string Id { get; }

        IFileSystemSnapshot LatestFileSystemSnapshot { get; } 

        IEnumerable<IFileSystemSnapshot> Snapshots { get; }

        IFileSystemSnapshot CreateSnapshot(IDirectory fileSystemState);

        IFileSystemDiff CompareSnapshots(string fromId, string toId);
    }
}