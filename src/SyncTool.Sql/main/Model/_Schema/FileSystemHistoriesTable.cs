using Grynwald.Utilities.Data;
using System.Data;

namespace SyncTool.Sql.Model
{
    static class FileSystemHistoriesTable
    {
        public enum Column
        {
            Id,
            Name,
            Version
        }

        public const string Name = "FileSystemHistories";
        
        public static void Create(IDbConnection connection, DatabaseLimits limits)
        {
            // Name should be treated as case-insensitive and must be unique
            connection.ExecuteNonQuery($@"                
                CREATE TABLE {Name} (            
                    {Column.Id}             INTEGER PRIMARY KEY AUTO_INCREMENT,
                    {Column.Name}           VARCHAR(500) CHARACTER SET utf8 COLLATE utf8_general_ci UNIQUE NOT NULL,
                    {Column.Version}        INTEGER NOT NULL DEFAULT 0)
            ");
        }
    }
}
