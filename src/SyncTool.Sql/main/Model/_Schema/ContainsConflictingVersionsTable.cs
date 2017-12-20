using Grynwald.Utilities.Data;
using System.Data;

namespace SyncTool.Sql.Model
{
    /// <summary>
    /// Table to model the 1:N relationship reuslting from the ConflictingVersions property in <see cref="SyncConflictDo"/>
    /// </summary>
    static class ContainsConflictingVersionsTable
    {
        public const string Name = "ContainsConflictingVersions";

        public enum Column
        {
            SyncConflictId,
            FileReferenceId
        }

        public static void Create(IDbConnection connection, DatabaseLimits limits)
        {
            connection.ExecuteNonQuery($@"
                CREATE TABLE {Name} (
                    {Column.SyncConflictId}  INTEGER NOT NULL,
                    {Column.FileReferenceId} INTEGER,
                    FOREIGN KEY ({Column.SyncConflictId})  REFERENCES {SyncConflictsTable.Name}({SyncConflictsTable.Column.Id}),
                    FOREIGN KEY ({Column.FileReferenceId}) REFERENCES {FileReferencesTable.Name}({FileReferencesTable.Column.Id}),
                    CONSTRAINT {Name}_Unique UNIQUE({Column.SyncConflictId},{Column.FileReferenceId}) 
                );               
            ");
        }
    }
}
