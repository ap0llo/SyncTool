using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SyncTool.FileSystem.Versioning;
using SyncTool.Sql.Model;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace SyncTool.Sql.Services
{
    class SqlMultiFileSystemHistoryService : AbstractMultiFileSystemHistoryService
    {
        private readonly IClock m_Clock;
        readonly ILogger<SqlMultiFileSystemHistoryService> m_Logger;
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
            [NotNull] IClock clock, 
            [NotNull] ILogger<SqlMultiFileSystemHistoryService> logger,
            [NotNull] IHistoryService historyService,
            [NotNull] MultiFileSystemSnapshotRepository repository, 
            [NotNull] Func<MultiFileSystemSnapshotDo, SqlMultiFileSystemSnapshot> snapshotFactory) : base(logger, historyService)
        {
            m_Clock = clock ?? throw new ArgumentNullException(nameof(clock));
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            m_HistoryService = historyService ?? throw new ArgumentNullException(nameof(historyService));
            m_SnapshotFactory = snapshotFactory ?? throw new ArgumentNullException(nameof(snapshotFactory));
            m_Repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }


        public override IMultiFileSystemSnapshot CreateSnapshot()
        {
            m_Logger.LogDebug("Creating MultiFileSystemSnapshot");

            var snapshotDo = new MultiFileSystemSnapshotDo()
            {
                SnapshotIds = m_HistoryService
                                .Items
                                .Select(history => (history.Name, history.LatestFileSystemSnapshot?.Id))
                                .ToList(),
                CreationUnixTimeTicks = m_Clock.GetCurrentInstant().ToUnixTimeTicks()
            };

            if (!snapshotDo.SnapshotIds.Any())
            {
                m_Logger.LogWarning("No snpashots in any history found, skipping aborting creation of snapshot");
                return null;
            }

            m_Repository.AddSnapshot(snapshotDo);
            m_Logger.LogDebug($"MultiFileSystemSnapshot {snapshotDo.Id} added to database");

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
