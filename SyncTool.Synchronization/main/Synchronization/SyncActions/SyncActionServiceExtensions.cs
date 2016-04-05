// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

namespace SyncTool.Synchronization.SyncActions
{
    public static class SyncActionServiceExtensions
    {

        public static void Add(this ISyncActionService service, params SyncAction[] syncActions)
        {
            service.Add(syncActions);
        }

        public static void Update(this ISyncActionService service, params SyncAction[] syncActions)
        {
            service.Update(syncActions);
        }

        public static void Remove(this ISyncActionService service, params SyncAction[] syncActions)
        {
            service.Remove(syncActions);
        }
    }
}