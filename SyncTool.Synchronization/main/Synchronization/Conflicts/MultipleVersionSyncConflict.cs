// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using SyncTool.FileSystem;

namespace SyncTool.Synchronization.Conflicts
{
    public sealed class MultipleVersionSyncConflict : SyncConflict
    {
        public override string FilePath => ConflictedFiles.First().Path;

        public IEnumerable<IFile> ConflictedFiles { get; }


        public MultipleVersionSyncConflict(params IFile[] conflictedFiles)
        {
            if (conflictedFiles == null)
            {
                throw new ArgumentNullException(nameof(conflictedFiles));
            }
            if (conflictedFiles.Length < 2)
            {
                throw new ArgumentException("Enumeration of conflicted files must at least contain two items", nameof(conflictedFiles));
            }

            // TODO: Check that all conflicts have the same path

            this.ConflictedFiles = conflictedFiles;
        }


        public override void Accept<T>(ISyncConflictVisitor<T> visitor, T parameter) => visitor.Visit(this, parameter);
    }
}