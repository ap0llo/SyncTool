// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using SyncTool.FileSystem;

namespace SyncTool.Synchronization
{
    public class ModificationDeletionConflictSyncAction : ConflictSyncAction
    {

        public IFile ModifiedFile { get; }

        public IFile DeletedFile { get; }


        public ModificationDeletionConflictSyncAction(IFile modifiedFile, IFile deletedFile)
        {
            if (modifiedFile == null)
            {
                throw new ArgumentNullException(nameof(modifiedFile));
            }
            if (deletedFile == null)
            {
                throw new ArgumentNullException(nameof(deletedFile));
            }
            this.ModifiedFile = modifiedFile;
            this.DeletedFile = deletedFile;
        }

        public override void Accept(ISyncActionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}