// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------
namespace SyncTool.Synchronization.Conflicts
{
    public interface ISyncConflictVisitor<T>
    {
        void Visit(MultipleVersionSyncConflict conflict, T parameter);

        void Visit(ModificationDeletionSyncConflict conflict, T parameter);

    }
}