using System.Data;

namespace SyncTool.Sql.Model.Tables
{
    static class FileSystemHistoriesTable
    {
        public enum Column
        {
            Id,
            Name,
            NormalizedName,
            Version
        }

        public const string Name = "FileSystemHistories";
        
        public static void Create(IDbConnection connection)
        {
            connection.ExecuteNonQuery($@"                
                CREATE TABLE IF NOT EXISTS {Name} (            
                    {Column.Id}             INTEGER PRIMARY KEY,
                    {Column.Name}           TEXT NOT NULL,
                    {Column.NormalizedName} TEXT UNIQUE NOT NULL,
                    {Column.Version}        INTEGER NOT NULL DEFAULT 0)
            ");
        }
    }
}
