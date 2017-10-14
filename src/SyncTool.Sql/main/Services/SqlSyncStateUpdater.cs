using JetBrains.Annotations;
using SyncTool.FileSystem;
using SyncTool.Sql.Model;
using SyncTool.Synchronization.State;
using SyncTool.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SyncTool.Sql.Services
{
    class SqlSyncStateUpdater : ISyncStateUpdater
    {
        readonly string m_NewSnapshotId;
        readonly SyncStateRepository m_Repository;
        readonly Func<SyncActionDo, SqlSyncAction> m_SyncActionFactory;
        readonly Func<SyncConflictDo, SqlSyncConflict> m_SyncConflictFactory;
        readonly SyncStateDo m_InitialState;

        readonly HashSet<ISyncAction> m_InitalSyncActions = new HashSet<ISyncAction>();
        readonly HashSet<ISyncAction> m_CurrentActions = new HashSet<ISyncAction>();

        readonly HashSet<ISyncConflict> m_InitialConflicts = new HashSet<ISyncConflict>();
        readonly HashSet<ISyncConflict> m_CurrentConflicts = new HashSet<ISyncConflict>();


        public string LastSyncSnapshotId => m_InitialState.SnapshotId;
        

        public SqlSyncStateUpdater(
            [NotNull] string newSnapshotId,
            [NotNull] SyncStateRepository repository,
            [NotNull] Func<SyncActionDo, SqlSyncAction> syncActionFactory,
            [NotNull] Func<SyncConflictDo, SqlSyncConflict> syncConflictFactory)
        {
            if (String.IsNullOrWhiteSpace(newSnapshotId))
                throw new ArgumentException("Value must not be empty", nameof(newSnapshotId));

            m_NewSnapshotId = newSnapshotId;
            m_Repository = repository ?? throw new ArgumentNullException(nameof(repository));
            m_SyncActionFactory = syncActionFactory ?? throw new ArgumentNullException(nameof(syncActionFactory));
            m_SyncConflictFactory = syncConflictFactory ?? throw new ArgumentNullException(nameof(syncConflictFactory));

            m_InitialState = m_Repository.GetSyncState();
            m_Repository.LoadActions(m_InitialState);
            m_Repository.LoadConflicts(m_InitialState);

            m_InitalSyncActions = m_InitialState.Actions.Select(syncActionFactory).Cast<ISyncAction>().ToHashSet();
            m_InitialConflicts = m_InitialState.Conflicts.Select(syncConflictFactory).Cast<ISyncConflict>().ToHashSet();

            m_CurrentActions = m_InitalSyncActions.ToHashSet();
            m_CurrentConflicts = m_InitialConflicts.ToHashSet();
        }



        public void AddConflict(string path, IEnumerable<FileReference> currentFileReferences) =>
            m_CurrentConflicts.Add(new SyncConflict(m_NewSnapshotId, currentFileReferences));

        public void AddSyncAction(string historyName, FileReference fileReference, FileReference selectedVersion)
        {
            // either fileReference or selectedVersion is != null
            var path = fileReference?.Path ?? selectedVersion.Path;
            var action = new SyncAction(m_NewSnapshotId, path, fileReference, selectedVersion);
            m_CurrentActions.Add(action);
        }


        public ISyncConflict GetConflictOrDefault(string path) =>
            m_CurrentConflicts.SingleOrDefault(c => StringComparer.OrdinalIgnoreCase.Equals(path, c.Path));

        public IReadOnlyCollection<ISyncAction> GetActions(string path) =>
            m_CurrentActions.Where(a => StringComparer.OrdinalIgnoreCase.Equals(path, a.Path)).ToArray();

        public void Remove(ISyncAction syncAction) => m_CurrentActions.Remove(syncAction);

        public void Remove(ISyncConflict conflict) => m_CurrentConflicts.Remove(conflict);

        public bool TryApply()
        {
            var actions = m_CurrentActions.Select(SyncActionDo.FromSyncAction).ToList();
            var conflicts = m_CurrentConflicts.Select(SyncConflictDo.FromSyncConflict).ToList();

            var stateDo = new SyncStateDo()
            {
                SnapshotId = m_NewSnapshotId,
                Version = m_InitialState.Version + 1,
                Actions = actions,
                Conflicts = conflicts
            };

            try
            {
                m_Repository.UpdateSyncState(stateDo);
                return true;
            }
            catch (DatabaseUpdateException)
            {
                return false;
            }
        }

        public void Dispose()
        {
        }

    }
}
