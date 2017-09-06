using System.Data;

namespace SyncTool.Sql.Model.Tables
{
    static class FileSystemSnapshotsTable
    {
        public const string Name = "FileSystemSnapshots";

        public enum Column
        {
            Id,
            HistoryId,
            SequenceNumber,
            CreationTimeTicks,
            RootDirectoryInstanceId,
            TmpId
        }

        public static void Create(IDbConnection connection, DatabaseLimits limits)
        {
            connection.ExecuteNonQuery($@"
                CREATE TABLE {Name}(
                    {Column.Id}                      INTEGER PRIMARY KEY,
                    {Column.HistoryId}               INTEGER NOT NULL,
                    {Column.SequenceNumber}          INTEGER UNIQUE NOT NULL,
                    {Column.CreationTimeTicks}       INTEGER NOT NULL,
                    {Column.RootDirectoryInstanceId} INTEGER NOT NULL,
                    {Column.TmpId}                   TEXT UNIQUE,
                    FOREIGN KEY ({Column.HistoryId})               REFERENCES {FileSystemHistoriesTable.Name}({FileSystemHistoriesTable.Column.Id})
                    FOREIGN KEY ({Column.RootDirectoryInstanceId}) REFERENCES {DirectoryInstancesTable.Name}({DirectoryInstancesTable.Column.Id}) );
            ");
        }
    }
}
