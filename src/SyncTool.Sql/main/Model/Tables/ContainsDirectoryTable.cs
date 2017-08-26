using System.Data;

namespace SyncTool.Sql.Model.Tables
{
    static class ContainsDirectoryTable
    {
        public const string Name = "ContainsDirectory";

        public enum Column
        {
            ParentId,
            ChildId
        }
        
        public static void Create(IDbConnection connection)
        {
            connection.ExecuteNonQuery($@"
                CREATE TABLE IF NOT EXISTS {Name} (
                    {Column.ParentId} INTEGER NOT NULL,
                    {Column.ChildId}  INTEGER NOT NULL,
                    FOREIGN KEY ({Column.ParentId}) REFERENCES {DirectoryInstancesTable.Name}({DirectoryInstancesTable.Column.Id}),
                    FOREIGN KEY ({Column.ChildId})  REFERENCES {DirectoryInstancesTable.Name}({DirectoryInstancesTable.Column.Id}),
                    CONSTRAINT {Name}_Unique UNIQUE({Column.ParentId},{Column.ChildId}) );
            ");
        }
    }
}
