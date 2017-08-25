using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using static SyncTool.Sql.Model.TypeMapper;

namespace SyncTool.Sql.Model
{
    public class SnapshotRepository
    {
        const string s_IncludesFileInstance = "IncludesFileInstance";
        const string s_SnapshotId = "SnapshotId";
        const string s_FileInstanceId = "FileInstanceId";
        const string s_TmpId = "tmpId";
        const string s_FileId = "FileId";

        readonly IDatabaseContextFactory m_ConnectionFactory;


        public SnapshotRepository(IDatabaseContextFactory connectionFactory)
        {
            m_ConnectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));

            //TODO: Should happen on first access??
            using (var connection = m_ConnectionFactory.OpenConnection())
            {
                connection.ExecuteNonQuery($@"
                    CREATE TABLE {Table<FileSystemSnapshotDo>()}(
                        {nameof(FileSystemSnapshotDo.Id)} INTEGER PRIMARY KEY,
                        {nameof(FileSystemSnapshotDo.HistoryId)} INTEGER NOT NULL,
                        {nameof(FileSystemSnapshotDo.SequenceNumber)} INTEGER UNIQUE NOT NULL,
                        {nameof(FileSystemSnapshotDo.CreationTimeTicks)} INTEGER NOT NULL,
                        {nameof(FileSystemSnapshotDo.RootDirectoryInstanceId)} INTEGER NOT NULL,
                        {s_TmpId} TEXT UNIQUE,
                        FOREIGN KEY ({nameof(FileSystemSnapshotDo.HistoryId)}) REFERENCES {Table<FileSystemHistoryDo>()}({nameof(FileSystemHistoryDo.Id)})
                        FOREIGN KEY ({nameof(FileSystemSnapshotDo.RootDirectoryInstanceId)}) REFERENCES {Table<DirectoryInstanceDo>()}({nameof(DirectoryInstanceDo.Id)})
                    );

                    CREATE TABLE {s_IncludesFileInstance}(
                        {s_SnapshotId} INTEGER NOT NULL,
                        {s_FileInstanceId} INTEGER NOT NULL,
                        FOREIGN KEY ({s_SnapshotId}) REFERENCES {Table<FileSystemSnapshotDo>()}({nameof(FileSystemSnapshotDo.Id)}),
                        FOREIGN KEY ({s_FileInstanceId}) REFERENCES {Table<FileInstanceDo>()}({nameof(FileInstanceDo.Id)}),
                        CONSTRAINT {s_IncludesFileInstance}_Unique UNIQUE({s_SnapshotId},{s_FileInstanceId})
                    );                    
                ");
            }
        }


        public FileSystemSnapshotDo GetSnapshotOrDefault(int historyId, int id)
        {
            using (var connection = m_ConnectionFactory.OpenConnection())
            {
                return connection.QuerySingleOrDefault<FileSystemSnapshotDo>($@"
                    SELECT *
                    FROM {Table<FileSystemSnapshotDo>()}
                    WHERE {nameof(FileSystemSnapshotDo.Id)} = @id AND
                          {nameof(FileSystemSnapshotDo.HistoryId)} = @historyId;
                ",
                new { historyId = historyId, id = id });
            }
        }

        public FileSystemSnapshotDo GetLatestSnapshotOrDefault(int historyId)
        {
            using (var connection = m_ConnectionFactory.OpenConnection())
            {
                return connection.QueryFirstOrDefault<FileSystemSnapshotDo>($@"
                    SELECT *
                    FROM {Table<FileSystemSnapshotDo>()}
                    WHERE {nameof(FileSystemSnapshotDo.HistoryId)} = @historyId
                    ORDER BY {nameof(FileSystemSnapshotDo.SequenceNumber)} DESC
                    LIMIT 2
                ",
                new { historyId = historyId });
            }
        }

        public IEnumerable<FileSystemSnapshotDo> GetSnapshots(int historyId)
        {
            using (var connection = m_ConnectionFactory.OpenConnection())
            {
                return connection.Query<FileSystemSnapshotDo>($@"
                    SELECT *
                    FROM {Table<FileSystemSnapshotDo>()}
                    WHERE {nameof(FileSystemSnapshotDo.HistoryId)} = @historyId;
                ",
                new { historyId  = historyId });
            }                
        }

        public void AddSnapshot(FileSystemSnapshotDo snapshot)
        {
            //TODO: make sure no other snapshots were added for the history ?? Is this a Problem?            
            
            using (var connection = m_ConnectionFactory.OpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                var tmpId = Guid.NewGuid().ToString();
                var inserted = connection.QuerySingle<FileSystemSnapshotDo>($@"
                        INSERT INTO {Table<FileSystemSnapshotDo>()} (                          
                            {nameof(FileSystemSnapshotDo.HistoryId)} ,
                            {nameof(FileSystemSnapshotDo.SequenceNumber)},
                            {nameof(FileSystemSnapshotDo.CreationTimeTicks)} ,
                            {nameof(FileSystemSnapshotDo.RootDirectoryInstanceId)},
                            {s_TmpId}
                        )
                        VALUES (@historyId, (SELECT count(*) FROM {Table<FileSystemSnapshotDo>()}), @ticks, @directoryId, @tmpId);

                        SELECT *
                        FROM {Table<FileSystemSnapshotDo>()}
                        WHERE {s_TmpId} = @tmpId;

                        UPDATE {Table<FileSystemSnapshotDo>()}
                        SET {s_TmpId} = NULL WHERE {s_TmpId} = @tmpId;
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
                        
                        INSERT INTO {s_IncludesFileInstance} ({s_SnapshotId}, {s_FileInstanceId})
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
            using (var connection = m_ConnectionFactory.OpenConnection())
            {
                var files = connection.Query<FileInstanceDo>($@"
                    SELECT * 
                    FROM {Table<FileInstanceDo>()}
                    WHERE {nameof(FileInstanceDo.Id)} IN (
                        SELECT {s_FileInstanceId}
                        FROM {s_IncludesFileInstance}
                        WHERE {s_SnapshotId} = @snapshotId
                    );
                ",
                new { snapshotId = snapshot.Id });

                snapshot.IncludedFiles = files.ToList();
            }
        }

        public FileSystemSnapshotDo GetPrecedingSnapshot(FileSystemSnapshotDo snapshot)
        {
            using (var connection = m_ConnectionFactory.OpenConnection())
            {
                return connection.QueryFirstOrDefault<FileSystemSnapshotDo>($@"
                    SELECT *
                    FROM {Table<FileSystemSnapshotDo>()}
                    WHERE {nameof(FileSystemSnapshotDo.HistoryId)} = @historyId AND
                          {nameof(FileSystemSnapshotDo.SequenceNumber)} < @sequenceNumber
                    ORDER BY {nameof(FileSystemSnapshotDo.SequenceNumber)} DESC
                    LIMIT 2
                ",
                new { historyId = snapshot.HistoryId , sequenceNumber = snapshot.SequenceNumber });
            }
        }

        public IEnumerable<string> GetChangedFiles(FileSystemSnapshotDo snapshot)
        {
            using (var connection = m_ConnectionFactory.OpenConnection())
            {                                                                
                var query = $@"

                    -- query database for preceding snapshot
                    WITH precedingSnapshotId AS 
                    (
                        SELECT {nameof(FileSystemSnapshotDo.Id)} FROM {Table<FileSystemSnapshotDo>()}
                        WHERE {nameof(FileSystemSnapshotDo.HistoryId)} = {snapshot.HistoryId} AND
                              {nameof(FileSystemSnapshotDo.SequenceNumber)} < {snapshot.SequenceNumber}
                        ORDER BY {nameof(FileSystemSnapshotDo.SequenceNumber)} DESC
                        LIMIT 1 
                    ),

                    -- get ids of file instances included in the current snapshot
                    fileInstanceIds AS 
                    (
                        SELECT {s_FileInstanceId} FROM {s_IncludesFileInstance}
                        WHERE {s_SnapshotId} = {snapshot.Id} 
                    ),
                    
                    -- get ids of file instances included in the preceding snapshot
                    previousFileInstanceIds AS 
                    (
                        SELECT {s_FileInstanceId} FROM {s_IncludesFileInstance}
                        WHERE {s_SnapshotId} IN precedingSnapshotId
                    ),

                    -- find the instances that are *not* 
                    -- included in both the current and the preceding snapshot
                    -- and select the ids of the correspondig file's id
                    changedFileIds AS 
                    (
                        SELECT DISTINCT {s_FileId} FROM {Table<FileInstanceDo>()}
                        WHERE 
                        (
                            {nameof(FileInstanceDo.Id)} IN fileInstanceIds AND
                            {nameof(FileInstanceDo.Id)} NOT IN previousFileInstanceIds
                        ) 
                        OR 
                        (
                            {nameof(FileInstanceDo.Id)} IN previousFileInstanceIds AND 
                            {nameof(FileInstanceDo.Id)} NOT IN fileInstanceIds
                        )
                    )

                    -- using the list of ids of changed files, get files from the database
                    SELECT * FROM {Table<FileDo>()}
                    WHERE {nameof(FileDo.Id)} IN changedFileIds;
                        
                ";                    
                return connection.Query<FileDo>(query).Select(x => x.Path).ToArray();
            }
        }
    }
}
