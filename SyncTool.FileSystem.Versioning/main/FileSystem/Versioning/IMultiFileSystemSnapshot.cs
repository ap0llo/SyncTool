// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace SyncTool.FileSystem.Versioning
{
    public interface IMultiFileSystemSnapshot
    {
        string Id { get; }

        IEnumerable<string> HistoryNames { get; }

        IFileSystemSnapshot GetSnapshot(string historyName);

        string GetSnapshotId(string historyName);
    }
}