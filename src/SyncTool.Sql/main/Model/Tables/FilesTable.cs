using JetBrains.Annotations;
using SyncTool.FileSystem;
using System.Data;

namespace SyncTool.Sql.Model.Tables
{
    public static class FilesTable
    {
        public const string Name = "Files";

        public enum Column
        {
            Id,
            Name,
            NormalizedPath,
            Path
        }

        public class Record
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public string NormalizedPath { get; set; }

            public string Path { get; set; }
            
            [UsedImplicitly]
            public Record() { }

            public static Record FromFile(IFile file) 
                => new Record()
                {
                    Name = file.Name,
                    Path = file.Path,
                    NormalizedPath = file.Path.NormalizeCaseInvariant(),
                };
        }

        public static void Create(IDbConnection connection)
        {
            connection.ExecuteNonQuery($@"
                CREATE TABLE {Name} (                
                    {Column.Id}             INTEGER PRIMARY KEY,
                    {Column.Name}           TEXT NOT NULL,
                    {Column.NormalizedPath} TEXT UNIQUE NOT NULL,
                    {Column.Path}           TEXT NOT NULL);
            ");
        }
    }
}
