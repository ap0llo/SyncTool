using Grynwald.Utilities.Data;
using System.Data;

namespace SyncTool.Sql.Model
{
    static class DirectoryInstancesTable
    {
        public const string Name = "DirectoryInstances";
        
        public enum Column
        {
            Id,
            DirectoryId,
            TmpId
        }

        public static void Create(IDbConnection connection, DatabaseLimits limits)
        {
            connection.ExecuteNonQuery($@"
                CREATE TABLE {Name} (
                    {Column.Id}          INTEGER PRIMARY KEY AUTO_INCREMENT,
                    {Column.DirectoryId} INTEGER NOT NULL,    
                    {Column.TmpId}       varchar(40) UNIQUE,
                    FOREIGN KEY ({Column.DirectoryId}) REFERENCES {DirectoriesTable.Name}({DirectoriesTable.Column.Id}));
            ");
        }
    }
}
