using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Data;

namespace SyncTool.Sql.Model.Tables
{
    public static class DirectoryInstancesTable
    {
        public const string Name = "DirectoryInstances";
        
        public enum Column
        {
            Id,
            DirectoryId,
            TmpId
        }

        public class Record
        {
            public int Id { get; set; }

            public DirectoriesTable.Record Directory { get; set; }

            public List<DirectoryInstancesTable.Record> Directories { get; set; }

            public List<FileInstancesTable.Record> Files { get; set; }

            [UsedImplicitly]
            public Record() { }

            public Record(DirectoriesTable.Record directory)
            {
                Directory = directory ?? throw new ArgumentNullException(nameof(directory));
                Directories = new List<DirectoryInstancesTable.Record>();
                Files = new List<FileInstancesTable.Record>();
            }
        }

        public static void Create(IDbConnection connection)
        {
            connection.ExecuteNonQuery($@"
                CREATE TABLE {Name} (
                    {Column.Id}          INTEGER PRIMARY KEY,
                    {Column.DirectoryId} INTEGER NOT NULL,    
                    {Column.TmpId}       TEXT UNIQUE,
                    FOREIGN KEY ({Column.DirectoryId}) REFERENCES {DirectoriesTable.Name}({DirectoriesTable.Column.Id}));
            ");
        }
    }
}
