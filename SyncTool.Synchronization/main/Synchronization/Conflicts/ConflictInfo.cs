// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Newtonsoft.Json;
using SyncTool.FileSystem;
using SyncTool.Synchronization.State;

namespace SyncTool.Synchronization.Conflicts
{
    public sealed class ConflictInfo
    {        
        public string FilePath { get; }
        
        public HistorySnapshotIdCollection SnapshotIds { get; }



        public ConflictInfo(string filePath, HistorySnapshotIdCollection snapshotIds)
        {
            PathValidator.EnsureIsValidFilePath(filePath);
            PathValidator.EnsureIsRootedPath(filePath);

            FilePath = filePath;
            SnapshotIds = snapshotIds;
        }

        
         
    }
}