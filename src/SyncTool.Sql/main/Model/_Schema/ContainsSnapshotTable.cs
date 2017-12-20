using Grynwald.Utilities.Data;
using System.Data;

namespace SyncTool.Sql.Model
{
    static class ContainsSnapshotTable
    {
        public const string Name = "ContainsSnapshot";

        public enum Column
        {
            MultiFileSystemSnapshotId,
            HistoryName,
            SnapshotId
        }
        
        public static void Create(IDbConnection connection, DatabaseLimits limits)
        {
            // snapshots are referened by history name and id and intentionally not by a foreign key to the FileSystemSnapshots table
            // becasue SqlMultiFileSystemHistoryService is implemented on top of IHistoryService
            // and there is no guarantee that the history service is actually a service implemented on top of the same database.
            // This way, the implementation of IHistoryService can be swapped without affecting the IMultiFileSystemHistoryService implementation

            connection.ExecuteNonQuery($@"
                CREATE TABLE {Name} (
                    {Column.MultiFileSystemSnapshotId}  INTEGER NOT NULL,
                    {Column.HistoryName}                VARCHAR(500) NOT NULL,
                    {Column.SnapshotId}                 VARCHAR(500),
                    FOREIGN KEY ({Column.MultiFileSystemSnapshotId})    REFERENCES {MultiFileSystemSnapshotsTable.Name}({MultiFileSystemSnapshotsTable.Column.Id}),                    
                    CONSTRAINT {Name}_Unique UNIQUE({Column.MultiFileSystemSnapshotId},{Column.HistoryName},{Column.SnapshotId}) );
            ");
        }
    }
}
