// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using SyncTool.Common;

namespace SyncTool.Synchronization.Conflicts
{
    public interface IConflictService : IItemService<string, ConflictInfo>
    {
        void AddItems(IEnumerable<ConflictInfo> conflicts);
        
        void RemoveItems(IEnumerable<ConflictInfo> conflicts);
             
    }
}