using SyncTool.Utilities;
using System;
using System.Data;
using System.Linq;

namespace SyncTool.Sql.Model.Tables
{
    static class PathFilterTable
    {
        const string Name = "PathFilter";

        public enum Column
        {
            Path
        }

        public static string CreateTemporary(IDbConnection connection, DatabaseLimits limits, string[] pathFilter)
        {
            if (pathFilter == null)
                throw new ArgumentNullException(nameof(pathFilter));

            
            connection.ExecuteNonQuery($@"
                        DROP TABLE IF EXISTS {Name};
                        CREATE TABLE {Name} (
                            {Column.Path} varchar(700) PRIMARY KEY            
                        )
            ");
            
            foreach (var segment in pathFilter.GetSegments(limits.MaxParameterCount))
            {

                var query = $@"
                    INSERT INTO {Name} ({Column.Path}) 
                    VALUES 
                    {
                        segment
                            .Select((_, index) => $"(@path{index})")
                            .JoinToString(",")
                    };";
                
                var parameters = segment.Select((path, index) => ($"path{index}", (object) path));
                
                connection.ExecuteNonQuery(query, parameters.ToArray());
            }

            return Name;
        }

    }
}
