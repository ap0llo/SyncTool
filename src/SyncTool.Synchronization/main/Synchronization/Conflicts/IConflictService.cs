// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using SyncTool.Common;

namespace SyncTool.Synchronization.Conflicts
{
    //TODO: Rename to ISyncConflictService 
    public interface IConflictService : IItemService<string, ConflictInfo>
    {
        // TODO: Check for duplicate paths in list
        void AddItems(IEnumerable<ConflictInfo> conflicts);

        // TODO: Check for duplicate paths in list
        void RemoveItems(IEnumerable<ConflictInfo> conflicts);
             
    }
}