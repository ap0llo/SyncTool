// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using SyncTool.FileSystem;

namespace SyncTool.Synchronization
{
    public sealed class RemoveFileSyncAction : ResolvedSyncAction
    {
        public override string FilePath => RemovedFile.Path;

        public IFile RemovedFile { get; }


        public RemoveFileSyncAction(SyncParticipant target, IFile removedFile) : base(target)
        {
            if (removedFile == null)
            {
                throw new ArgumentNullException(nameof(removedFile));
            }
            this.RemovedFile = removedFile;
        }


        public override void Accept<T>(ISyncActionVisitor<T> visitor, T parameter)
        {
            visitor.Visit(this, parameter);
        }
    }
}