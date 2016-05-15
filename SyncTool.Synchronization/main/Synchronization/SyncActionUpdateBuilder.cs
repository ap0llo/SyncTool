// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using SyncTool.Synchronization.Conflicts;
using SyncTool.Synchronization.SyncActions;

namespace SyncTool.Synchronization
{
    class SyncActionUpdateBuilder
    {
        readonly LinkedList<SyncAction> m_AddedSyncActions = new LinkedList<SyncAction>();     
        readonly List<SyncAction> m_UpdatedSyncActions = new List<SyncAction>(); 
        readonly List<ConflictInfo> m_AddedConflicts = new List<ConflictInfo>(); 

        public void AddSyncAction(SyncAction action)
        {
            m_AddedSyncActions.AddLast(action);
        }

        public void UpdateSyncAction(SyncAction syncAction)
        {
            m_UpdatedSyncActions.Add(syncAction);
        }


        public void UpdateSyncActions(IEnumerable<SyncAction> syncActions)
        {
            m_UpdatedSyncActions.AddRange(syncActions);
        }


        public void AddConflict(ConflictInfo conflictInfo)
        {
            m_AddedConflicts.Add(conflictInfo);
        }


        public void Apply(ISyncActionService syncActionService, IConflictService conflictService)
        {
            syncActionService.AddItems(m_AddedSyncActions);       
            syncActionService.UpdateItems(m_UpdatedSyncActions);

            conflictService.AddItems(m_AddedConflicts);
        }
    }
}