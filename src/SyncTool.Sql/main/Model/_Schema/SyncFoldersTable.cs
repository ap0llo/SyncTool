using Grynwald.Utilities.Data;
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
            // Name should be treated as case-insensitive and is the primary key
            connection.ExecuteNonQuery($@"
                CREATE TABLE {Name} (
                    {Column.Name}    VARCHAR(500) CHARACTER SET utf8 COLLATE utf8_general_ci PRIMARY KEY,
                    {Column.Path}    TEXT,
                    {Column.Version} INTEGER NOT NULL DEFAULT 0)
            ");
        }
    }
}
