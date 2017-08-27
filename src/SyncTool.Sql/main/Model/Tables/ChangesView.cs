using System.Data;

namespace SyncTool.Sql.Model.Tables
{
    static class ChangesView
    {
        const string Name = "Changes";

        public enum Column
        {
            FileId,
            CurrentId,
            PreviousId,
            CurrentLastWriteTimeTicks,
            PreviousLastWriteTimeTicks,
            CurrentLength,
            PreviousLength
        }


        public static string CreateTemporary(IDbConnection connection, FileSystemSnapshotDo snapshot)
        {            
            const string s_PrecedingSnapshotId = "precedingSnapshotId";
            const string s_FileInstanceIds = "fileInstanceIds";
            const string s_PreviousFileInstanceIds = "previousFileInstanceIds";
            const string s_Current = "current";
            const string s_Previous = "previous";
            string s_FileInstances = $"(SELECT * FROM {FileInstancesTable.Name} WHERE {FileInstancesTable.Column.Id} IN {s_FileInstanceIds})";
            string s_PreviousFileInstances = $"(SELECT * FROM {FileInstancesTable.Name} WHERE {FileInstancesTable.Column.Id} IN {s_PreviousFileInstanceIds})";

            connection.ExecuteNonQuery($@"                    

                CREATE TEMPORARY VIEW {Name} AS 

                    -- query database for preceding snapshot
                    WITH {s_PrecedingSnapshotId} AS 
                    (
                        SELECT {FileSystemSnapshotsTable.Column.Id} FROM {FileSystemSnapshotsTable.Name}
                        WHERE {FileSystemSnapshotsTable.Column.HistoryId} = {snapshot.HistoryId} AND
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
                    )                        

                    -- (SQLite does not support a full outer join, so we need to combine two left joins)  
                    SELECT 
                        {s_Current}.{FileInstancesTable.Column.FileId}              AS {Column.FileId},
                        {s_Current}.{FileInstancesTable.Column.Id}                  AS {Column.CurrentId},
                        {s_Current}.{FileInstancesTable.Column.LastWriteTimeTicks}  AS {Column.CurrentLastWriteTimeTicks},
                        {s_Current}.{FileInstancesTable.Column.Length}              AS {Column.CurrentLength},
                        {s_Previous}.{FileInstancesTable.Column.Id}                 AS {Column.PreviousId},
                        {s_Previous}.{FileInstancesTable.Column.LastWriteTimeTicks} AS {Column.PreviousLastWriteTimeTicks},
                        {s_Previous}.{FileInstancesTable.Column.Length}             AS {Column.PreviousLength}
                    FROM {s_FileInstances} AS {s_Current} 
                    LEFT OUTER JOIN {s_PreviousFileInstances} AS {s_Previous}
                    ON {s_Current}.{FileInstancesTable.Column.FileId} = {s_Previous}.{FileInstancesTable.Column.FileId}                        
                    UNION
                        SELECT 
                            {s_Previous}.{FileInstancesTable.Column.FileId}             AS {Column.FileId},
                            {s_Current}.{FileInstancesTable.Column.Id}                  AS {Column.CurrentId},
                            {s_Current}.{FileInstancesTable.Column.LastWriteTimeTicks}  AS {Column.CurrentLastWriteTimeTicks},
                            {s_Current}.{FileInstancesTable.Column.Length}              AS {Column.CurrentLength},
                            {s_Previous}.{FileInstancesTable.Column.Id}                 AS {Column.PreviousId},
                            {s_Previous}.{FileInstancesTable.Column.LastWriteTimeTicks} AS {Column.PreviousLastWriteTimeTicks},
                            {s_Previous}.{FileInstancesTable.Column.Length}             AS {Column.PreviousLength}
                        FROM {s_PreviousFileInstances} AS {s_Previous}
                        LEFT OUTER JOIN {s_FileInstances} AS {s_Current}                                 
                        ON {s_Current}.{FileInstancesTable.Column.FileId} = {s_Previous}.{FileInstancesTable.Column.FileId};                                                                
            ");

            return Name;

        }
    }
}
