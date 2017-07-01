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