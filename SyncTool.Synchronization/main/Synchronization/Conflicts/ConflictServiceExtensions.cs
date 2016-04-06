// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

namespace SyncTool.Synchronization.Conflicts
{
    public static class ConflictServiceExtensions
    {
        public static void AddItems(this IConflictService service, params ConflictInfo[] syncActions) => service.AddItems(syncActions);

        public static void RemoveItems(this IConflictService service, params ConflictInfo[] syncActions) => service.RemoveItems(syncActions);
    }
}