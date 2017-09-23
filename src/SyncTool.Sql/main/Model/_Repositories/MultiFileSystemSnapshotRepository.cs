using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SyncTool.Utilities;

namespace SyncTool.Sql.Model
{
    public class MultiFileSystemSnapshotRepository
    {
        readonly Database m_Database;


        public IEnumerable<MultiFileSystemSnapshotDo> Items => m_Database.Query<MultiFileSystemSnapshotDo>($"SELECT * FROM {MultiFileSystemSnapshotsTable.Name};").ToArray();


        public MultiFileSystemSnapshotRepository([NotNull] Database database)
        {
            m_Database = database ?? throw new ArgumentNullException(nameof(database));
        }


        [CanBeNull]
        public MultiFileSystemSnapshotDo GetLatestSnapshotOrDefault()
        {
            return m_Database.QuerySingleOrDefault<MultiFileSystemSnapshotDo>($@"
                SELECT * FROM {MultiFileSystemSnapshotsTable.Name}
                ORDER BY {MultiFileSystemSnapshotsTable.Column.Id} DESC
                LIMIT 1;");                
        }

        [CanBeNull]
        public MultiFileSystemSnapshotDo GetSnapshotOrDefault(int id)
        {
            return m_Database.QuerySingleOrDefault<MultiFileSystemSnapshotDo>($@"
                SELECT * FROM {MultiFileSystemSnapshotsTable.Name}
                WHERE {MultiFileSystemSnapshotsTable.Column.Id} = @id",
                new { id = id });
        }

        [NotNull]
        public MultiFileSystemSnapshotDo GetSnapshot(int id)
        {
            return m_Database.QuerySingle<MultiFileSystemSnapshotDo>($@"
                SELECT * FROM {MultiFileSystemSnapshotsTable.Name}
                WHERE {MultiFileSystemSnapshotsTable.Column.Id} = @id",
                new { id = id });
        }

        public void LoadSnapshots([NotNull] MultiFileSystemSnapshotDo snapshotDo)
        {
            if (snapshotDo == null)
                throw new ArgumentNullException(nameof(snapshotDo));

            lock (snapshotDo)
            {
                if (snapshotDo.SnapshotIds != null)
                    return;                

                snapshotDo.SnapshotIds = m_Database.Query($@"
                    SELECT {ContainsSnapshotTable.Column.HistoryName} AS historyId,
                           {ContainsSnapshotTable.Column.SnapshotId} AS snapshotId
                    FROM {ContainsSnapshotTable.Name}
                    WHERE {ContainsSnapshotTable.Column.MultiFileSystemSnapshotId} = @id",
                    new { id = snapshotDo.Id } )               
                .Select(record => (historyId: (string)record.historyId, snapshotId: (string)record.snapshotId))
                .ToList();
            }
        }

        public void AddSnapshot(MultiFileSystemSnapshotDo snapshotDo)
        {
            int id;
            using (var connection = m_Database.OpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                var tmpId = Guid.NewGuid().ToString();
                id = connection.ExecuteScalar<int>($@"
                    INSERT INTO {MultiFileSystemSnapshotsTable.Name} ({MultiFileSystemSnapshotsTable.Column.TmpId})
                    VALUES (@tmpId);

                    SELECT {MultiFileSystemSnapshotsTable.Column.Id}
                    FROM {MultiFileSystemSnapshotsTable.Name}
                    WHERE  {MultiFileSystemSnapshotsTable.Column.TmpId} = @tmpId",

                    ("tmpId", tmpId));


                foreach (var segment in snapshotDo.SnapshotIds.ToArray().GetSegments((m_Database.Limits.MaxParameterCount / 2) - 1))
                {
                    var query = $@"
                        INSERT INTO {ContainsSnapshotTable.Name}
                        (
                            {ContainsSnapshotTable.Column.MultiFileSystemSnapshotId},
                            {ContainsSnapshotTable.Column.HistoryName},
                            {ContainsSnapshotTable.Column.SnapshotId}
                        )
                        VALUES
                        {
                            segment
                                .Select((_, index) => $"(@multiFileSystemSnapshotId, @historyName{index}, @snapshotId{index})")
                                .JoinToString(",")
                        };
                    ";

                    var parameters = new List<(string, object)>()
                    {
                        ("multiFileSystemSnapshotId", id)
                    };


                    var i = 0;
                    foreach (var item in segment)
                    {
                        parameters.Add(($"historyName{i}", item.historyName));
                        parameters.Add(($"snapshotId{i}", item.snapshotId));
                        i += 1;
                    }

                    connection.ExecuteNonQuery(query, parameters.ToArray());                       
                }

                transaction.Commit();
            }

            snapshotDo.Id = id;
        }
    }
}