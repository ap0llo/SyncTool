using System;
using System.Data;

namespace SyncTool.Sql.Model
{
    static class SyncActionsTable
    {
        public const string Name = "SyncActions";

        public enum Column
        {
            Id,
            SnapshotId,
            FromVersionFileReferenceId,
            ToVersionFileReferenceId
        }

        public static void Create(IDbConnection connection, DatabaseLimits limits)
        {
            connection.ExecuteNonQuery($@"
                CREATE TABLE {Name} (
                    {Column.Id}                         INTEGER PRIMARY KEY AUTO_INCREMENT,
                    {Column.SnapshotId}                 VARCHAR(500) DEFAULT NULL,
                    {Column.FromVersionFileReferenceId} INTEGER,
                    {Column.ToVersionFileReferenceId}   INTEGER,
                    FOREIGN KEY ({Column.FromVersionFileReferenceId}) REFERENCES {FileReferencesTable.Name}({FileReferencesTable.Column.Id}),
                    FOREIGN KEY ({Column.ToVersionFileReferenceId})   REFERENCES {FileReferencesTable.Name}({FileReferencesTable.Column.Id})
                );               
            ");
        }
    }
}
