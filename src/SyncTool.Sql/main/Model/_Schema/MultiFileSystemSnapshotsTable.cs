using Grynwald.Utilities.Data;
using System.Data;

namespace SyncTool.Sql.Model
{
    static class MultiFileSystemSnapshotsTable
    {
        public const string Name = "MultiFileSystemSnapshots";

        public enum Column
        {
            Id,
            CreationUnixTimeTicks,
            TmpId
        }

        public static void Create(IDbConnection connection, DatabaseLimits limits)
        {
            connection.ExecuteNonQuery($@"
                CREATE TABLE {Name}(
                    {Column.Id}                     INTEGER PRIMARY KEY AUTO_INCREMENT,
                    {Column.CreationUnixTimeTicks}  BIGINT NOT NULL,
                    {Column.TmpId}                  VARCHAR(40) UNIQUE );
            ");
        }
    }
}
