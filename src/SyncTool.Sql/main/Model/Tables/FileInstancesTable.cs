using System.Data;

namespace SyncTool.Sql.Model.Tables
{
    static class FileInstancesTable
    {
        public const string Name = "FileInstances";

        public enum Column
        {
            Id,
            FileId,
            LastWriteTimeTicks,
            Length
        }

        public static void Create(IDbConnection connection, DatabaseLimits limits)
        {
            connection.ExecuteNonQuery($@"
                CREATE TABLE {Name} (                
                    {Column.Id}                 INTEGER PRIMARY KEY AUTO_INCREMENT,
                    {Column.FileId}             INTEGER NOT NULL,
                    {Column.LastWriteTimeTicks} BIGINT NOT NULL,
                    {Column.Length}             INTEGER NOT NULL,
                    FOREIGN KEY ({Column.FileId}) REFERENCES {FilesTable.Name}({FilesTable.Column.Id}),
                    CONSTRAINT FileInstance_Unique UNIQUE (
                        {Column.FileId}, 
                        {Column.LastWriteTimeTicks}, 
                        {Column.Length}
                    )); 
            ");
        }
    }
}
