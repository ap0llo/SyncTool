// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Newtonsoft.Json;
using SyncTool.FileSystem;

namespace SyncTool.Synchronization.Conflicts
{
    public sealed class ConflictInfo
    {        
        public string FilePath { get; }
        
        public IReadOnlyDictionary<string, string> SnapshotIds { get; }



        public ConflictInfo(string filePath, IReadOnlyDictionary<string, string> snapshotIds)
        {
            PathValidator.EnsureIsValidFilePath(filePath);
            PathValidator.EnsureIsRootedPath(filePath);

            FilePath = filePath;
            SnapshotIds = snapshotIds;
        }

        
         
    }
}