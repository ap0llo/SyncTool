using System.Data;

namespace SyncTool.Sql.Model.Tables
{
    static class ContainsFileTable
    {
        public const string Name = "ContainsFile";

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
                    FOREIGN KEY ({Column.ChildId})  REFERENCES {FileInstancesTable.Name}({FileInstancesTable.Column.Id}),
                    CONSTRAINT {Name}_Unique UNIQUE({Column.ParentId},{Column.ChildId}) );
            ");
        }
    }
}
