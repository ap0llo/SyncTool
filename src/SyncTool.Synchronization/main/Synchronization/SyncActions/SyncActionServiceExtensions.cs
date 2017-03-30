// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

namespace SyncTool.Synchronization.SyncActions
{
    public static class SyncActionServiceExtensions
    {

        public static void AddItems(this ISyncActionService service, params SyncAction[] syncActions)
        {
            service.AddItems(syncActions);
        }

        public static void UpdateItems(this ISyncActionService service, params SyncAction[] syncActions)
        {
            service.UpdateItems(syncActions);
        }

        public static void RemoveItems(this ISyncActionService service, params SyncAction[] syncActions)
        {
            service.RemoveItems(syncActions);
        }
    }
}