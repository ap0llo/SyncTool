using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;

using SyncTool.Sql.Model.Tables;

namespace SyncTool.Sql.Model
{
    public class SnapshotRepository
    {
        private class ChangeTableRecord
        {
            public int FileId { get; set; }

            public int? CurrentId { get; set; }

            public int? PreviousId { get; set; }

            public long? CurrentLastWriteTimeTicks { get; set; }

            public long? PreviousLastWriteTimeTicks { get; set; }

            public long? CurrentLength { get; set; }
            
            public long? PreviousLength { get; set; }

            public void Deconstruct(out FileInstanceDo previous, out FileInstanceDo current, out int fileId)
            {
                previous = PreviousId.HasValue
                    ? new FileInstanceDo() { Id = PreviousId.Value, LastWriteTimeTicks = PreviousLastWriteTimeTicks.Value, Length = PreviousLength.Value }
                    : null;

                current = CurrentId.HasValue
                    ? new FileInstanceDo() { Id = CurrentId.Value, LastWriteTimeTicks = CurrentLastWriteTimeTicks.Value, Length = CurrentLength.Value }
                    : null;

                fileId = FileId;
            }
        }
                                
        readonly Database m_Database;


        public SnapshotRepository(Database database)
        {
            m_Database = database ?? throw new ArgumentNullException(nameof(database));
        }


        public FileSystemSnapshotDo GetSnapshotOrDefault(int historyId, int id)
        {
            return m_Database.QuerySingleOrDefault<FileSystemSnapshotDo>($@"
                SELECT *
                FROM {FileSystemSnapshotsTable.Name}
                WHERE {FileSystemSnapshotsTable.Column.Id} = @id AND
                        {FileSystemSnapshotsTable.Column.HistoryId} = @historyId;
            ",
            new { historyId = historyId, id = id });
        }

        public FileSystemSnapshotDo GetLatestSnapshotOrDefault(int historyId)
        {
            return m_Database.QueryFirstOrDefault<FileSystemSnapshotDo>($@"
                SELECT *
                FROM {FileSystemSnapshotsTable.Name}
                WHERE {FileSystemSnapshotsTable.Column.HistoryId} = @historyId
                ORDER BY {FileSystemSnapshotsTable.Column.SequenceNumber} DESC
                LIMIT 2",
                new { historyId = historyId });
        }

        public IEnumerable<FileSystemSnapshotDo> GetSnapshots(int historyId)
        {
            return m_Database.Query<FileSystemSnapshotDo>($@"
                SELECT *
                FROM {FileSystemSnapshotsTable.Name}
                WHERE {FileSystemSnapshotsTable.Column.HistoryId} = @historyId;",
                new { historyId = historyId });
        }

        public void AddSnapshot(FileSystemSnapshotDo snapshot)
        {
            //TODO: make sure no other snapshots were added for the history ?? Is this a Problem?            

            using (var connection = m_Database.OpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                var tmpId = Guid.NewGuid().ToString();
                var inserted = connection.QuerySingle<FileSystemSnapshotDo>($@"
                        INSERT INTO {FileSystemSnapshotsTable.Name} 
                        (                          
                            {FileSystemSnapshotsTable.Column.HistoryId} ,
                            {FileSystemSnapshotsTable.Column.SequenceNumber},
                            {FileSystemSnapshotsTable.Column.CreationTimeTicks} ,
                            {FileSystemSnapshotsTable.Column.RootDirectoryInstanceId},
                            {FileSystemSnapshotsTable.Column.TmpId}
                        )
                        VALUES 
                        (   
                            @historyId, 
                            (SELECT count(*) FROM {FileSystemSnapshotsTable.Name}), 
                            @ticks, 
                            @directoryId, 
                            @tmpId
                        );

                        SELECT *
                        FROM {FileSystemSnapshotsTable.Name}
                        WHERE {FileSystemSnapshotsTable.Column.TmpId} = @tmpId;

                        UPDATE {FileSystemSnapshotsTable.Name}
                        SET {FileSystemSnapshotsTable.Column.TmpId} = NULL 
                        WHERE {FileSystemSnapshotsTable.Column.TmpId} = @tmpId;
                ",
                new
                {
                    historyId = snapshot.HistoryId,
                    ticks = snapshot.CreationTimeTicks,
                    directoryId = snapshot.RootDirectoryInstanceId,
                    tmpId = tmpId
                });


                foreach (var fileInstance in snapshot.IncludedFiles)
                {
                    connection.ExecuteNonQuery($@"
                        
                        INSERT INTO {IncludesFileInstanceTable.Name} 
                        (
                            {IncludesFileInstanceTable.Column.SnapshotId}, 
                            {IncludesFileInstanceTable.Column.FileInstanceId}
                        )
                        VALUES (@snapshotId, @fileInstanceId)",

                        ("snapshotId", inserted.Id), 
                        ("fileInstanceId", fileInstance.Id)
                    );
                }

                transaction.Commit();

                snapshot.Id = inserted.Id;
                snapshot.SequenceNumber = inserted.SequenceNumber;
            }
        }

        public void LoadIncludedFiles(FileSystemSnapshotDo snapshot)
        {
            var files = m_Database.Query<FileInstanceDo>($@"
                SELECT * 
                FROM {FileInstancesTable.Name}
                WHERE {FileInstancesTable.Column.Id} IN 
                (
                    SELECT {IncludesFileInstanceTable.Column.FileInstanceId}
                    FROM {IncludesFileInstanceTable.Name}
                    WHERE {IncludesFileInstanceTable.Column.SnapshotId} = @snapshotId
                );
            ",
            new { snapshotId = snapshot.Id });

            snapshot.IncludedFiles = files.ToList();            
        }

        public FileSystemSnapshotDo GetPrecedingSnapshot(FileSystemSnapshotDo snapshot)
        {
            return m_Database.QueryFirstOrDefault<FileSystemSnapshotDo>($@"
                SELECT *
                FROM {FileSystemSnapshotsTable.Name}
                WHERE {FileSystemSnapshotsTable.Column.HistoryId} = @historyId AND
                        {FileSystemSnapshotsTable.Column.SequenceNumber} < @sequenceNumber
                ORDER BY {FileSystemSnapshotsTable.Column.SequenceNumber} DESC
                LIMIT 2",
                new { historyId = snapshot.HistoryId, sequenceNumber = snapshot.SequenceNumber });
        }

        public IEnumerable<string> GetChangedFiles(FileSystemSnapshotDo snapshot)
        {
            const string s_PrecedingSnapshotId = "precedingSnapshotId";
            const string s_FileInstanceIds = "fileInstanceIds";
            const string s_PreviousFileInstanceIds = "previousFileInstanceIds";
            const string s_ChangedFileIds = "changedFileIds";

            var query = $@"

                -- query database for preceding snapshot
                WITH {s_PrecedingSnapshotId} AS 
                (
                    SELECT {FileSystemSnapshotsTable.Column.Id} 
                    FROM {FileSystemSnapshotsTable.Name}
                    WHERE  {FileSystemSnapshotsTable.Column.HistoryId} = {snapshot.HistoryId} AND
                            {FileSystemSnapshotsTable.Column.SequenceNumber} < {snapshot.SequenceNumber}
                    ORDER BY {FileSystemSnapshotsTable.Column.SequenceNumber} DESC
                    LIMIT 1 
                ),

                -- get ids of file instances included in the current snapshot
                {s_FileInstanceIds} AS 
                (
                    SELECT {IncludesFileInstanceTable.Column.FileInstanceId} 
                    FROM {IncludesFileInstanceTable.Name}
                    WHERE {IncludesFileInstanceTable.Column.SnapshotId} = {snapshot.Id} 
                ),
                    
                -- get ids of file instances included in the preceding snapshot
                {s_PreviousFileInstanceIds} AS 
                (
                    SELECT {IncludesFileInstanceTable.Column.FileInstanceId} 
                    FROM {IncludesFileInstanceTable.Name}
                    WHERE {IncludesFileInstanceTable.Column.SnapshotId} IN {s_PrecedingSnapshotId}
                ),

                -- find the instances that are *not* 
                -- included in both the current and the preceding snapshot
                -- and select the ids of the correspondig file's id
                {s_ChangedFileIds} AS 
                (
                    SELECT DISTINCT {FileInstancesTable.Column.FileId} FROM {FileInstancesTable.Name}
                    WHERE 
                    (
                        {FileInstancesTable.Column.Id} IN {s_FileInstanceIds} AND
                        {FileInstancesTable.Column.Id} NOT IN {s_PreviousFileInstanceIds}
                    ) 
                    OR 
                    (
                        {FileInstancesTable.Column.Id} IN {s_PreviousFileInstanceIds} AND 
                        {FileInstancesTable.Column.Id} NOT IN {s_FileInstanceIds}
                    )
                )

                -- using the list of ids of changed files, get files from the database
                SELECT * FROM {FilesTable.Name}
                WHERE {FilesTable.Column.Id} IN {s_ChangedFileIds};
                        
            ";

            return m_Database.Query<FileDo>(query).Select(x => x.Path).ToArray();
        }

        public IEnumerable<(FileInstanceDo previous, FileInstanceDo current)> GetChanges(FileSystemSnapshotDo snapshot, string[] pathFilter)
        {            
            using (var connection = m_Database.OpenConnection())
            {
                var changesView = ChangesView.CreateTemporary(connection, snapshot);
                var filesView = FilteredFilesView.CreateTemporary(connection, pathFilter);

                var fileDos = connection.Query<FileDo>($@"                    
                        SELECT * FROM {filesView}
                        WHERE {FilesTable.Column.Id} IN (SELECT DISTINCT {nameof(ChangeTableRecord.FileId)} FROM {changesView});")
                    .ToDictionary(record => record.Id);

                var changeQuery = $@"
                    SELECT * FROM {changesView}
                    WHERE 
                        (
                            {ChangesView.Column.CurrentId} IS NULL OR 
                            {ChangesView.Column.PreviousId} IS NULL OR 
                            {ChangesView.Column.CurrentId} != {ChangesView.Column.PreviousId}
                        )
                        AND 
                        (
                            {nameof(ChangeTableRecord.FileId)} IN (SELECT {FilteredFilesView.Column.Id} FROM {filesView})
                        )
                ;";

                var changes = new LinkedList<(FileInstanceDo, FileInstanceDo)>();
                foreach (var (previous, current, fileId) in connection.Query<ChangeTableRecord>(changeQuery))
                {
                    var fileDo = fileDos[fileId];                    

                    if(previous != null)
                        previous.File = fileDo;

                    if(current != null)
                        current.File = fileDo;

                    changes.AddLast((previous, current));
                }
                
                return changes;
            }
        }
    }
}
