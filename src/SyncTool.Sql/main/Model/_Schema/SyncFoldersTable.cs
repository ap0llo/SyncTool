using System.Data;

namespace SyncTool.Sql.Model
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

        public static void Create(IDbConnection connection, DatabaseLimits limits)
        {
            connection.ExecuteNonQuery($@"
                CREATE TABLE {Name} (
                    {Column.Name}    varchar(500) PRIMARY KEY,
                    {Column.Path}    TEXT,
                    {Column.Version} INTEGER NOT NULL DEFAULT 0)
            ");
        }
    }
}
