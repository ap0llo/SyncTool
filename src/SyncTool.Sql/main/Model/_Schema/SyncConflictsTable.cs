using Grynwald.Utilities.Data;
using System;
using System.Data;

namespace SyncTool.Sql.Model
{
    static class SyncConflictsTable
    {
        public const string Name = "SyncConflicts";

        public enum Column
        {
            Id,
            SnapshotId
        }

        public static void Create(IDbConnection connection, DatabaseLimits limits)
        {
            connection.ExecuteNonQuery($@"
                CREATE TABLE {Name} (
                    {Column.Id}  INTEGER PRIMARY KEY AUTO_INCREMENT,
                    {Column.SnapshotId} VARCHAR(500) DEFAULT NULL
                );               
            ");
        }
    }
}
