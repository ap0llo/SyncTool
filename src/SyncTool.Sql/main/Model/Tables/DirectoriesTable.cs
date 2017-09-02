using JetBrains.Annotations;
using SyncTool.FileSystem;
using System.Collections.Generic;
using System.Data;

namespace SyncTool.Sql.Model.Tables
{
    public static class DirectoriesTable
    {
        public const string Name = "Directories";

        public enum Column
        {
            Id,
            Name,
            NormalizedPath
        }

        public class Record
        {
            public int Id { get; set; }

            public string NormalizedPath { get; set; }

            public string Name { get; set; }

            public List<DirectoryInstancesTable.Record> Instances { get; set; } = new List<DirectoryInstancesTable.Record>();


            [UsedImplicitly]
            public Record() { }

            public static Record FromDirectory(IDirectory directory) 
                => new Record()
                {
                    Name = directory.Name,
                    NormalizedPath = directory.Path.NormalizeCaseInvariant(),
                };
        }

        public static void Create(IDbConnection connection)
        {
            connection.ExecuteNonQuery($@"
                CREATE TABLE {Name} (                
                    {Column.Id}             INTEGER PRIMARY KEY,
                    {Column.Name}           TEXT NOT NULL,
                    {Column.NormalizedPath} TEXT UNIQUE NOT NULL);
            ");
        }
    }
}
