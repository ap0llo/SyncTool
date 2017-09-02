using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;

using SyncTool.Sql.Model.Tables;

namespace SyncTool.Sql.Model
{
    public class SnapshotRepository
    {

                                
        readonly Database m_Database;


        public SnapshotRepository(Database database)
        {
            m_Database = database ?? throw new ArgumentNullException(nameof(database));
        }


        public FileSystemSnapshotsTable.Record GetSnapshotOrDefault(int historyId, int id)
        {
            return m_Database.QuerySingleOrDefault<FileSystemSnapshotsTable.Record>($@"
                SELECT *
                FROM {FileSystemSnapshotsTable.Name}
                WHERE {FileSystemSnapshotsTable.Column.Id} = @id AND
                        {FileSystemSnapshotsTable.Column.HistoryId} = @historyId;
            ",
            new { historyId = historyId, id = id });
        }

        public FileSystemSnapshotsTable.Record GetLatestSnapshotOrDefault(int historyId)
        {
            return m_Database.QueryFirstOrDefault<FileSystemSnapshotsTable.Record>($@"
                SELECT *
                FROM {FileSystemSnapshotsTable.Name}
                WHERE {FileSystemSnapshotsTable.Column.HistoryId} = @historyId
                ORDER BY {FileSystemSnapshotsTable.Column.SequenceNumber} DESC
                LIMIT 2",
                new { historyId = historyId });
        }

        public IEnumerable<FileSystemSnapshotsTable.Record> GetSnapshots(int historyId)
        {
            return m_Database.Query<FileSystemSnapshotsTable.Record>($@"
                SELECT *
                FROM {FileSystemSnapshotsTable.Name}
                WHERE {FileSystemSnapshotsTable.Column.HistoryId} = @historyId;",
                new { historyId = historyId });
        }

        public void AddSnapshot(FileSystemSnapshotsTable.Record snapshot)
        {
            //TODO: make sure no other snapshots were added for the history ?? Is this a Problem?            

            using (var connection = m_Database.OpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                var tmpId = Guid.NewGuid().ToString();
                var inserted = connection.QuerySingle<FileSystemSnapshotsTable.Record>($@"
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

        public void LoadIncludedFiles(FileSystemSnapshotsTable.Record snapshot)
        {
            var files = m_Database.Query<FileInstancesTable.Record>($@"
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

        public FileSystemSnapshotsTable.Record GetPrecedingSnapshot(FileSystemSnapshotsTable.Record snapshot)
        {
            return m_Database.QueryFirstOrDefault<FileSystemSnapshotsTable.Record>($@"
                SELECT *
                FROM {FileSystemSnapshotsTable.Name}
                WHERE {FileSystemSnapshotsTable.Column.HistoryId} = @historyId AND
                        {FileSystemSnapshotsTable.Column.SequenceNumber} < @sequenceNumber
                ORDER BY {FileSystemSnapshotsTable.Column.SequenceNumber} DESC
                LIMIT 2",
                new { historyId = snapshot.HistoryId, sequenceNumber = snapshot.SequenceNumber });
        }

        public IEnumerable<string> GetChangedFiles(FileSystemSnapshotsTable.Record snapshot)
        {
            using (var connection = m_Database.OpenConnection())
            {
                var changesView = ChangesView.CreateTemporary(connection, snapshot);
                               
                return connection.Query<string>($@"
                        SELECT {FilesTable.Column.Path} 
                        FROM {FilesTable.Name}
                        WHERE {FilesTable.Column.Id} IN 
                        (
                            SELECT {ChangesView.Column.FileId} 
                            FROM {changesView}                                                 
                        )")
                    .ToArray();
            }            
        }

        public IEnumerable<(FileInstancesTable.Record previous, FileInstancesTable.Record current)> GetChanges(FileSystemSnapshotsTable.Record snapshot, string[] pathFilter)
        {            
            using (var connection = m_Database.OpenConnection())
            {
                var changesView = ChangesView.CreateTemporary(connection, snapshot);
                var filesView = FilteredFilesView.CreateTemporary(connection, pathFilter);

                var fileDos = connection.Query<FilesTable.Record>($@"                    
                        SELECT * FROM {filesView}
                        WHERE {FilesTable.Column.Id} IN (SELECT DISTINCT {ChangesView.Column.FileId} FROM {changesView});")
                    .ToDictionary(record => record.Id);

                var changeQuery = $@"
                    SELECT * FROM {changesView}
                    WHERE {ChangesView.Column.FileId} IN (SELECT {FilteredFilesView.Column.Id} FROM {filesView})
                        
                ;";

                var changes = new LinkedList<(FileInstancesTable.Record, FileInstancesTable.Record)>();
                foreach (var (previous, current, fileId) in connection.Query<ChangesView.Record>(changeQuery))
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

        /// <summary>
        /// Gets all the snapshots between the specified snapshots
        /// </summary>
        /// <param name="fromSnapshot">The start of the range (exclusive)</param>
        /// <param name="toSnapshot">The end of the range of snapshots to return (inclusive)</param>
        public IEnumerable<FileSystemSnapshotsTable.Record> GetSnapshotRange(FileSystemSnapshotsTable.Record fromSnapshot, FileSystemSnapshotsTable.Record toSnapshot)
        {
            if (fromSnapshot.HistoryId != toSnapshot.HistoryId)
                throw new ArgumentException("Cannot get range between snapshots of different histories");            

            return m_Database.Query<FileSystemSnapshotsTable.Record>($@"
                SELECT *
                FROM {FileSystemSnapshotsTable.Name}
                WHERE {FileSystemSnapshotsTable.Column.Id} > @fromId AND
                      {FileSystemSnapshotsTable.Column.Id} <= @toId AND
                      {FileSystemSnapshotsTable.Column.HistoryId} = @historyId
                ORDER BY {FileSystemSnapshotsTable.Column.SequenceNumber} ASC;
                ",
                new { fromId = fromSnapshot.Id, toId = toSnapshot.Id, historyId = fromSnapshot.HistoryId });
        }

        /// <summary>
        /// Gets all the snapshots up to the specified snapshot
        /// </summary>        
        /// <param name="toSnapshot">The end of the range of snapshots to return (inclusive)</param>
        public IEnumerable<FileSystemSnapshotsTable.Record> GetSnapshotRange(FileSystemSnapshotsTable.Record toSnapshot)
        {            
            return m_Database.Query<FileSystemSnapshotsTable.Record>($@"
                SELECT *
                FROM {FileSystemSnapshotsTable.Name}
                WHERE {FileSystemSnapshotsTable.Column.Id} <= @toId AND
                      {FileSystemSnapshotsTable.Column.HistoryId} = @historyId
                ORDER BY {FileSystemSnapshotsTable.Column.SequenceNumber} ASC;
                ",
                new { toId = toSnapshot.Id, historyId = toSnapshot.HistoryId });
        }
    }
}
