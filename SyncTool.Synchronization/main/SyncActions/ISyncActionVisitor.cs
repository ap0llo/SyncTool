// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------
namespace SyncTool.Synchronization
{
    public interface ISyncActionVisitor
    {
        void Visit(MultipleVersionConflictSyncAction action);

        void Visit(ModificationDeletionConflictSyncAction action);

        void Visit(ReplaceFileSyncAction action);

        void Visit(AddFileSyncAction action);

        void Visit(RemoveFileSyncAction action);

    }
}