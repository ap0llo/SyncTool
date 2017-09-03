using System;
using System.Data;

namespace SyncTool.Sql.Model.Tables
{
    static class PathFilterTable
    {
        const string Name = "PathFilter";

        public enum Column
        {
            Path
        }

        public static string CreateTemporary(IDbConnection connection, string[] pathFilter)
        {
            if (pathFilter == null)
                throw new ArgumentNullException(nameof(pathFilter));

            
            connection.ExecuteNonQuery($@"
                        CREATE TEMPORARY TABLE {Name} (
                            {Column.Path} TEXT PRIMARY KEY            
                        )
            ");

            //TODO: Add batching of inserts
            foreach (var path in pathFilter)
            {
                connection.ExecuteNonQuery($@"
                    INSERT INTO {Name} ({Column.Path}) VALUES (@path);",
                    ("path", path)
                );
            }

            return Name;
        }

    }
}
