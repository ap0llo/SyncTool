using System.Data;

namespace SyncTool.Sql.Model.Tables
{
    public static class ChangesView
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

        public class Record
        {
            public int FileId { get; set; }

            public int? CurrentId { get; set; }

            public int? PreviousId { get; set; }

            public long? CurrentLastWriteTimeTicks { get; set; }

            public long? PreviousLastWriteTimeTicks { get; set; }

            public long? CurrentLength { get; set; }

            public long? PreviousLength { get; set; }

            public void Deconstruct(out FileInstancesTable.Record previous, out FileInstancesTable.Record current, out int fileId)
            {
                previous = PreviousId.HasValue
                    ? new FileInstancesTable.Record() { Id = PreviousId.Value, LastWriteTimeTicks = PreviousLastWriteTimeTicks.Value, Length = PreviousLength.Value }
                    : null;

                current = CurrentId.HasValue
                    ? new FileInstancesTable.Record() { Id = CurrentId.Value, LastWriteTimeTicks = CurrentLastWriteTimeTicks.Value, Length = CurrentLength.Value }
                    : null;

                fileId = FileId;
            }
        }

        public static string CreateTemporary(IDbConnection connection, FileSystemSnapshotsTable.Record snapshot)
        {            
            const string s_PrecedingSnapshotId = "precedingSnapshotId";
            const string s_FileInstanceIds = "fileInstanceIds";
            const string s_PreviousFileInstanceIds = "previousFileInstanceIds";
            const string s_Current = "current";
            const string s_Previous = "previous";
            string s_FileInstances = $"(SELECT * FROM {FileInstancesTable.Name} WHERE {FileInstancesTable.Column.Id} IN {s_FileInstanceIds})";
            string s_PreviousFileInstances = $"(SELECT * FROM {FileInstancesTable.Name} WHERE {FileInstancesTable.Column.Id} IN {s_PreviousFileInstanceIds})";
            const string s_UnfilteredChanged = Name + "_Unfiltered";

            connection.ExecuteNonQuery($@"                    

                CREATE TEMPORARY VIEW {s_UnfilteredChanged} AS 

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


                -- filter view to only include changes
                -- CurrentId NULL => file was deleted
                -- PreviousId NULL => file was added
                -- CurrentId != PreviousId => file was modified
                CREATE TEMPORARY VIEW {Name} AS
                    SELECT * FROM {s_UnfilteredChanged} 
                    WHERE   
                    (
                            {Column.CurrentId} IS NULL OR 
                            {Column.PreviousId} IS NULL OR 
                            {Column.CurrentId} != {ChangesView.Column.PreviousId}
                    );
            ");

            return Name;

        }
    }
}
