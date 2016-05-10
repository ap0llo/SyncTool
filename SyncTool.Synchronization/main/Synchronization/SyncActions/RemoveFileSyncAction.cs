// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using SyncTool.FileSystem;
using SyncTool.FileSystem.Versioning;

namespace SyncTool.Synchronization.SyncActions
{
    public sealed class RemoveFileSyncAction : SyncAction
    {
        public override string FilePath => RemovedFile.Path;

        public override ChangeType Type => ChangeType.Deleted;

        public override IFileReference FromVersion => RemovedFile;

        public override IFileReference ToVersion => null;


        public IFileReference RemovedFile { get; }


        public RemoveFileSyncAction(Guid id, string target,SyncActionState state, int syncPointId, IFileReference removedFile) : base(id, target, state, syncPointId)
        {
            if (removedFile == null)
            {
                throw new ArgumentNullException(nameof(removedFile));
            }
            this.RemovedFile = removedFile;
        }


        public override SyncAction WithState(SyncActionState state) => new RemoveFileSyncAction(Id, Target, state, SyncPointId, RemovedFile);
    }
}