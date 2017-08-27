using System.Data;

namespace SyncTool.Sql.Model.Tables
{
    static class SyncFoldersTable
    {
        public const string Name = "SyncFolders";

        public enum Column
        {
            Name,
            Path,
            Version
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
