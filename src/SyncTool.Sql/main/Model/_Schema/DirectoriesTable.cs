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
            connection.ExecuteNonQuery($@"
                CREATE TABLE {Name} (                
                    {Column.Id}             INTEGER PRIMARY KEY AUTO_INCREMENT,
                    {Column.Name}           TEXT NOT NULL,
                    {Column.Path}           VARCHAR(700) UNIQUE NOT NULL );
            ");
        }
    }
}
