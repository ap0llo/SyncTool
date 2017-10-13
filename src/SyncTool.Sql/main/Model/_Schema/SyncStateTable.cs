using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace SyncTool.Sql.Model
{
    static class SyncStateTable
    {
        public const string Name = "SyncState";

        public enum Column
        {
            Name,
            SnapshotId,
            Version
        }

        public static void Create(IDbConnection connection, DatabaseLimits limits)
        {
            // table is supposed to only have a single row
            // for that purpose, the "Name" column will always have 
            // the same value and has a UNIQUE constraint 

            connection.ExecuteNonQuery($@"
                CREATE TABLE {Name} (
                    {Column.Name}       VARCHAR(20) UNIQUE NOT NULL,                  
                    {Column.SnapshotId} VARCHAR(500) DEFAULT NULL,
                    {Column.Version}    BIGINT NOT NULL
                );

                INSERT INTO {Name} ({Column.Name}, {Column.Version}) 
                VALUES ('SyncState', 1);
            ");
        }

    }
}
