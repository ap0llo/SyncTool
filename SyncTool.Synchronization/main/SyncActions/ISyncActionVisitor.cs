// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------
namespace SyncTool.Synchronization
{
    public interface ISyncActionVisitor<T>
    {
        
        void Visit(ReplaceFileSyncAction action, T parameter);

        void Visit(AddFileSyncAction action, T parameter);

        void Visit(RemoveFileSyncAction action, T parameter);

    }
}