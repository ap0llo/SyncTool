using System;
using System.Collections.Generic;
using System.Text;
using SyncTool.Synchronization.State;
using SyncTool.Sql.Model;
using JetBrains.Annotations;
using System.Linq;

namespace SyncTool.Sql.Services
{
    class SqlSyncStateService : ISyncStateService
    {
        readonly SyncStateRepository m_SyncStateRepository;
        readonly Func<string, SqlSyncStateUpdater> m_UpdaterFactory;
        
        
        public string LastSyncSnapshotId => m_SyncStateRepository.GetSyncState().SnapshotId;

        public IReadOnlyCollection<SyncAction> Actions
        {
            get
            {
                var syncState = m_SyncStateRepository.GetSyncState();
                m_SyncStateRepository.LoadActions(syncState);
                return syncState.Actions.Select(a => a.ToSyncAction(m_SyncStateRepository)).ToArray();
            }
        }

        public IReadOnlyCollection<SyncConflict> Conflicts
        {
            get
            {
                var syncState = m_SyncStateRepository.GetSyncState();
                m_SyncStateRepository.LoadConflicts(syncState);
                return syncState.Conflicts.Select(c => c.ToSyncConflict(m_SyncStateRepository)).ToArray();
            }
        }

        
        public SqlSyncStateService(
            [NotNull] SyncStateRepository syncStateRepository,
            [NotNull] Func<string, SqlSyncStateUpdater> updaterFactory)
        {
            m_SyncStateRepository = syncStateRepository ?? throw new ArgumentNullException(nameof(syncStateRepository));
            m_UpdaterFactory = updaterFactory ?? throw new ArgumentNullException(nameof(updaterFactory));            
        }


        public ISyncStateUpdater BeginUpdate(string newSnapshotId) => m_UpdaterFactory.Invoke(newSnapshotId);
    }
}
