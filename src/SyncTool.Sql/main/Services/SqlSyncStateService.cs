using System;
using System.Collections.Generic;
using System.Text;
using SyncTool.Synchronization.State;

namespace SyncTool.Sql.Services
{
    public class SqlSyncStateService : ISyncStateService
    {
        readonly Func<SqlSyncStateUpdater> m_UpdaterFactory;

        public string LastSyncSnapshotId => throw new NotImplementedException();


        public SqlSyncStateService(Func<SqlSyncStateUpdater> updaterFactory)
        {
            m_UpdaterFactory = updaterFactory ?? throw new ArgumentNullException(nameof(updaterFactory));
        }

        public IEnumerable<ISyncAction> SyncActions => throw new NotImplementedException();

        public IEnumerable<IConflict> Conflicts => throw new NotImplementedException();

        public ISyncStateUpdater BeginUpdate() => m_UpdaterFactory.Invoke();
    }
}
