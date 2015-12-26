// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using SyncTool.FileSystem;

namespace SyncTool.Synchronization.Conflicts
{
    public sealed class ModificationDeletionSyncConflict : SyncConflict
    {

        public override string FilePath => ModifiedFile.Path;

        public IFile ModifiedFile { get; }

        public IFile DeletedFile { get; }


        public ModificationDeletionSyncConflict(IFile modifiedFile, IFile deletedFile)
        {
            if (modifiedFile == null)
            {
                throw new ArgumentNullException(nameof(modifiedFile));
            }
            if (deletedFile == null)
            {
                throw new ArgumentNullException(nameof(deletedFile));
            }
            if (!StringComparer.InvariantCultureIgnoreCase.Equals(modifiedFile.Path, deletedFile.Path))
            {
                throw new ArgumentException($"The paths of {nameof(modifiedFile)} and {nameof(deletedFile)} differ");
            }

            this.ModifiedFile = modifiedFile;
            this.DeletedFile = deletedFile;
        }

        
    }
}