// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using SyncTool.FileSystem;

namespace SyncTool.Synchronization
{
    public sealed class AddFileSyncAction : ResolvedSyncAction
    {

        public override string FilePath => NewFile.Path;

        public IFile NewFile { get; }

        public AddFileSyncAction(SyncParticipant target, IFile newFile) : base(target)
        {
            if (newFile == null)
            {
                throw new ArgumentNullException(nameof(newFile));
            }
            this.NewFile = newFile;
        }

        public override void Accept<T>(ISyncActionVisitor<T> visitor, T parameter)
        {
            visitor.Visit(this, parameter);
        }
    }
}