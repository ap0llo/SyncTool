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
            NormalizedPath
        }

        public static void Create(IDbConnection connection, DatabaseLimits limits)
        {
            connection.ExecuteNonQuery($@"
                CREATE TABLE {Name} (                
                    {Column.Id}             INTEGER PRIMARY KEY AUTO_INCREMENT,
                    {Column.Name}           TEXT NOT NULL,
                    {Column.NormalizedPath} varchar(700) UNIQUE NOT NULL);
            ");
        }
    }
}
