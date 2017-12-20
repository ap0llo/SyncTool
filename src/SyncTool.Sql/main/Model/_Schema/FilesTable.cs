using Grynwald.Utilities.Data;
using System.Data;

namespace SyncTool.Sql.Model
{
    static class FilesTable
    {
        public const string Name = "Files";

        public enum Column
        {
            Id,
            Name,
            Path
        }

        public static void Create(IDbConnection connection, DatabaseLimits limits)
        {
            connection.ExecuteNonQuery($@"
                CREATE TABLE {Name} (                
                    {Column.Id}             INTEGER PRIMARY KEY AUTO_INCREMENT,
                    {Column.Name}           VARCHAR(1000) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
                    {Column.Path}           VARCHAR(1000) CHARACTER SET utf8 COLLATE utf8_general_ci UNIQUE NOT NULL );
            ");
        }
    }
}
