using System.Data;

namespace SyncTool.Sql.Model
{
    static class IncludesFileInstanceTable
    {
        public const string Name = "IncludesFileInstance";

        public enum Column
        {
            SnapshotId,
            FileInstanceId
        }

        public static void Create(IDbConnection connection, DatabaseLimits limits)
        {
            connection.ExecuteNonQuery($@"                    
                CREATE TABLE {Name}(
                    {Column.SnapshotId}     INTEGER NOT NULL,
                    {Column.FileInstanceId} INTEGER NOT NULL,
                    FOREIGN KEY ({Column.SnapshotId})     REFERENCES {FileSystemSnapshotsTable.Name}({FileSystemSnapshotsTable.Column.Id}),
                    FOREIGN KEY ({Column.FileInstanceId}) REFERENCES {FileInstancesTable.Name}({FileInstancesTable.Column.Id}),
                    CONSTRAINT {Name}_Unique UNIQUE({Column.SnapshotId},{Column.FileInstanceId}) );                    
            ");
        }
    }
}
