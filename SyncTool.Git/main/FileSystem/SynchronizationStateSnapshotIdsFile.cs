// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using SyncTool.FileSystem;
using SyncTool.Git.Synchronization.Transfer;
using SyncTool.Synchronization.Transfer;

namespace SyncTool.Git.FileSystem
{
    public class SynchronizationStateSnapshotIdsFile : DataFile<SynchronizationStateSnapshotIds>
    {

        public const string FileName = "SnapshotIds.json";

        
        
        public SynchronizationStateSnapshotIdsFile(IDirectory parent, SynchronizationStateSnapshotIds content) : base(parent, FileName, content)
        {            
        }

        

        public override IFile WithParent(IDirectory newParent) => new SynchronizationStateSnapshotIdsFile(newParent, this.Content);
    }
}