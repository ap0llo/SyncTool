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
        
        public string SnapshotId { get; }


        public ConflictInfo(string filePath, string snapshotId)
        {
            PathValidator.EnsureIsValidFilePath(filePath);
            PathValidator.EnsureIsRootedPath(filePath);            

            FilePath = filePath;
            SnapshotId = snapshotId;
        }

        
         
    }
}