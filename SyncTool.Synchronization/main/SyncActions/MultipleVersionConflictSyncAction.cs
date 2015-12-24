// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using SyncTool.FileSystem;

namespace SyncTool.Synchronization
{
    public sealed class MultipleVersionConflictSyncAction : ConflictSyncAction
    {
        public IEnumerable<IFile> ConflictedFiles { get; }


        public MultipleVersionConflictSyncAction(params IFile[] conflictedFiles)
        {
            if (conflictedFiles == null)
            {
                throw new ArgumentNullException(nameof(conflictedFiles));
            }

            if (!conflictedFiles.Any())
            {
                throw new ArgumentException("Enumeration of conflicted files must not be empty", nameof(conflictedFiles));
            }

            this.ConflictedFiles = conflictedFiles;
        }


        public override void Accept(ISyncActionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}