using System.Data;

namespace SyncTool.Sql.Model.Tables
{
    static class FilesTable
    {
        public const string Name = "Files";

        public enum Column
        {
            Id,
            Name,
            NormalizedPath,
            Path
        }

        public static void Create(IDbConnection connection)
        {
            connection.ExecuteNonQuery($@"
                CREATE TABLE IF NOT EXISTS {Name} (                
                    {Column.Id}             INTEGER PRIMARY KEY,
                    {Column.Name}           TEXT NOT NULL,
                    {Column.NormalizedPath} TEXT UNIQUE NOT NULL,
                    {Column.Path}           TEXT NOT NULL);
            ");
        }
    }
}
