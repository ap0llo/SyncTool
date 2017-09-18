using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SyncTool.FileSystem.Versioning;
using SyncTool.Sql.Model;

namespace SyncTool.Sql.Services
{
    class SqlMultiFileSystemHistoryService : AbstractMultiFileSystemHistoryService
    {
        readonly IHistoryService m_HistoryService;
        readonly Func<MultiFileSystemSnapshotDo, SqlMultiFileSystemSnapshot> m_SnapshotFactory;
        readonly MultiFileSystemSnapshotRepository m_Repository;


        public override IMultiFileSystemSnapshot LatestSnapshot
        {
            get
            {
                var snapshotDo = m_Repository.GetLatestSnapshotOrDefault();
                return snapshotDo != null ? m_SnapshotFactory.Invoke(snapshotDo) : null;
            }
        }

        public override IEnumerable<IMultiFileSystemSnapshot> Snapshots => m_Repository.Items.Select(m_SnapshotFactory);
        

        public SqlMultiFileSystemHistoryService(
            [NotNull] IHistoryService historyService,
            [NotNull] MultiFileSystemSnapshotRepository repository, 
            [NotNull] Func<MultiFileSystemSnapshotDo, SqlMultiFileSystemSnapshot> snapshotFactory) : base(historyService)
        {
            m_HistoryService = historyService ?? throw new ArgumentNullException(nameof(historyService));
            m_SnapshotFactory = snapshotFactory ?? throw new ArgumentNullException(nameof(snapshotFactory));
            m_Repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }


        public override IMultiFileSystemSnapshot CreateSnapshot()
        {
            var snapshotDo = new MultiFileSystemSnapshotDo()
            {
                SnapshotIds = m_HistoryService
                                .Items
                                .Select(history => (history.Name, history.LatestFileSystemSnapshot?.Id))
                                .ToList()
            };

            if (!snapshotDo.SnapshotIds.Any())
                return null;

            m_Repository.AddSnapshot(snapshotDo);
            return m_SnapshotFactory.Invoke(snapshotDo);
        }


        protected override IMultiFileSystemSnapshot GetSnapshot(string id)
        {
            if (!int.TryParse(id, out var dbId))
                throw new SnapshotNotFoundException(id);

            var snapshotDo = m_Repository.GetSnapshotOrDefault(dbId);

            if (snapshotDo == null)
                throw new SnapshotNotFoundException(id);

            return m_SnapshotFactory.Invoke(snapshotDo);
        }

        protected override void AssertIsAncestor(string ancestorId, string descandantId)
        {
            var ancestor = m_Repository.GetSnapshot(int.Parse(ancestorId));
            var descandant = m_Repository.GetSnapshot(int.Parse(descandantId));

            if(ancestor.Id >= descandant.Id)
                throw new InvalidRangeException($"Snapshot {descandantId} is not an descendant of {ancestorId}");
        }
    }
}
