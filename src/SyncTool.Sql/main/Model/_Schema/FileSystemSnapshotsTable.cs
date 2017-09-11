﻿using System.Data;

namespace SyncTool.Sql.Model
{
    static class FileSystemSnapshotsTable
    {
        public const string Name = "FileSystemSnapshots";

        public enum Column
        {
            Id,
            HistoryId,            
            CreationTimeTicks,
            RootDirectoryInstanceId,
            TmpId
        }

        public static void Create(IDbConnection connection, DatabaseLimits limits)
        {
            connection.ExecuteNonQuery($@"
                CREATE TABLE {Name}(
                    {Column.Id}                      INTEGER PRIMARY KEY AUTO_INCREMENT,
                    {Column.HistoryId}               INTEGER NOT NULL,                    
                    {Column.CreationTimeTicks}       BIGINT NOT NULL,
                    {Column.RootDirectoryInstanceId} INTEGER NOT NULL,
                    {Column.TmpId}                   varchar(40) UNIQUE,
                    FOREIGN KEY ({Column.HistoryId})               REFERENCES {FileSystemHistoriesTable.Name}({FileSystemHistoriesTable.Column.Id}),
                    FOREIGN KEY ({Column.RootDirectoryInstanceId}) REFERENCES {DirectoryInstancesTable.Name}({DirectoryInstancesTable.Column.Id}) );
            ");
        }
    }
}
