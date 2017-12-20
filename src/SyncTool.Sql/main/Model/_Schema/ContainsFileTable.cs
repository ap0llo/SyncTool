using Grynwald.Utilities.Data;
using System.Data;

namespace SyncTool.Sql.Model
{
    static class ContainsFileTable
    {
        public const string Name = "ContainsFile";

        public enum Column
        {
            ParentId,
            ChildId
        }
        
        public static void Create(IDbConnection connection, DatabaseLimits limits)
        {
            connection.ExecuteNonQuery($@"
                CREATE TABLE {Name} (
                    {Column.ParentId} INTEGER NOT NULL,
                    {Column.ChildId}  INTEGER NOT NULL,
                    FOREIGN KEY ({Column.ParentId}) REFERENCES {DirectoryInstancesTable.Name}({DirectoryInstancesTable.Column.Id}),
                    FOREIGN KEY ({Column.ChildId})  REFERENCES {FileInstancesTable.Name}({FileInstancesTable.Column.Id}),
                    CONSTRAINT {Name}_Unique UNIQUE({Column.ParentId},{Column.ChildId}) );
            ");
        }
    }
}
