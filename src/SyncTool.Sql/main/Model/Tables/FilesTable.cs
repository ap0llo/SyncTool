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

        public static void Create(IDbConnection connection, DatabaseLimits limits)
        {
            connection.ExecuteNonQuery($@"
                CREATE TABLE {Name} (                
                    {Column.Id}             INTEGER PRIMARY KEY AUTO_INCREMENT,
                    {Column.Name}           TEXT NOT NULL,
                    {Column.NormalizedPath} varchar(700) UNIQUE NOT NULL,
                    {Column.Path}           TEXT NOT NULL);
            ");
        }
    }
}
