using SyncTool.Configuration;
using System.Data;

namespace SyncTool.Sql.Model.Tables
{
    public static class SyncFoldersTable
    {
        public const string Name = "SyncFolders";

        public enum Column
        {
            Name,
            Path,
            Version
        }

        public class Record
        {
            public string Name { get; set; }

            public string Path { get; set; }

            public int Version { get; set; }

            public SyncFolder ToSyncFolder() => new SyncFolder(Name) { Path = Path };

            public static Record FromSyncFolder(SyncFolder folder) 
                => new Record()
                {
                    Name = folder.Name,
                    Path = folder.Path
                };
        }

        public static void Create(IDbConnection connection)
        {
            connection.ExecuteNonQuery($@"
                CREATE TABLE {Name} (
                    {Column.Name}    TEXT PRIMARY KEY,
                    {Column.Path}    TEXT,
                    {Column.Version} INTEGER NOT NULL DEFAULT 0)
            ");
        }
    }
}
