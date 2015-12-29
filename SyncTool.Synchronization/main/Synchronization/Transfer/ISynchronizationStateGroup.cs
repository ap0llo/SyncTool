// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using SyncTool.Common;

namespace SyncTool.Synchronization.Transfer
{
    public interface ISynchronizationStateGroup : IGroup<ISynchronizationState>
    {
        new ISynchronizationState this[string name] { get; set; }
        
    }
}