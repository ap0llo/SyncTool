using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Data;

namespace SyncTool.Sql.Model.Tables
{
    public static class FileSystemSnapshotsTable
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

        public class Record
        {
            public int HistoryId { get; set; }

            // assigned automatically on db insert
            public int Id { get; set; }

            // assigned automatically on db insert
            public int SequenceNumber { get; set; }

            public long CreationTimeTicks { get; set; }

            public int RootDirectoryInstanceId { get; set; }

            public List<FileInstancesTable.Record> IncludedFiles { get; set; } = new List<FileInstancesTable.Record>();


            [UsedImplicitly]
            public Record() { }

            public Record(
                int historyId,
                long creationTimeTicks,
                int rootDirectoryInstanceId,
                List<FileInstancesTable.Record> includedFiles)
            {
                if (rootDirectoryInstanceId <= 0)
                    throw new ArgumentOutOfRangeException(nameof(rootDirectoryInstanceId));

                HistoryId = historyId;
                CreationTimeTicks = creationTimeTicks;
                RootDirectoryInstanceId = rootDirectoryInstanceId;
                IncludedFiles = includedFiles;
            }
        }

        public static void Create(IDbConnection connection)
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
