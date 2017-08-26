using System.Data;

namespace SyncTool.Sql.Model.Tables
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

        public static void Create(IDbConnection connection)
        {
            connection.ExecuteNonQuery($@"
                CREATE TABLE IF NOT EXISTS {Name} (                
                    {Column.Id}             INTEGER PRIMARY KEY,
                    {Column.Name}           TEXT NOT NULL,
                    {Column.NormalizedPath} TEXT UNIQUE NOT NULL);
            ");
        }
    }
}
