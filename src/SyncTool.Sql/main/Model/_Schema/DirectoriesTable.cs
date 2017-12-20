using Grynwald.Utilities.Data;
using System.Data;

namespace SyncTool.Sql.Model
{
    static class DirectoriesTable
    {
        public const string Name = "Directories";

        public enum Column
        {
            Id,
            Name,
            Path                
        }

        public static void Create(IDbConnection connection, DatabaseLimits limits)
        {
            // Path and Name should be treated case-insensitive for queries (and unique-constraints)
            // For MySQL this is actually the default behavour, however it does not hurt to specify it explicitly
            connection.ExecuteNonQuery($@"
                CREATE TABLE {Name} (                
                    {Column.Id}             INTEGER PRIMARY KEY AUTO_INCREMENT,
                    {Column.Name}           VARCHAR(1000) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
                    {Column.Path}           VARCHAR(1000) CHARACTER SET utf8 COLLATE utf8_general_ci UNIQUE NOT NULL );
            ");
        }
    }
}
