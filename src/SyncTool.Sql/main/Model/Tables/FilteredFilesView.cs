using System.Data;

namespace SyncTool.Sql.Model.Tables
{
    static class FilteredFilesView
    {
        const string Name = "FilteredFiles";

        public enum Column
        {
            Id,
            Name,
            NormalizedPath,
            Path
        }

        public static string CreateTemporary(IDbConnection connection, DatabaseLimits limits, string[] pathFilter)
        {
            if (pathFilter != null)
            {
                var pathFilterTable = PathFilterTable.CreateTemporary(connection, limits, pathFilter);

                connection.ExecuteNonQuery($@"
                    DROP VIEW IF EXISTS {Name} ;
                    CREATE VIEW {Name} AS 
                        SELECT 
                            {FilesTable.Column.Id} AS {Column.Id},         
                            {FilesTable.Column.Name} AS {Column.Name},            
                            {FilesTable.Column.Path} AS {Column.Path}
                        FROM {FilesTable.Name}
                        WHERE lower({FilesTable.Column.Path}) IN 
                        (
                            SELECT {PathFilterTable.Column.Path} FROM {pathFilterTable}
                        );
                ");
            }
            else
            {
                connection.ExecuteNonQuery($@"
                    DROP VIEW IF EXISTS {Name};
                    CREATE VIEW {Name} AS 
                        SELECT 
                            {FilesTable.Column.Id} AS {Column.Id},         
                            {FilesTable.Column.Name} AS {Column.Name},            
                            {FilesTable.Column.Path} AS {Column.Path}
                        FROM {FilesTable.Name}
                ");
            }

            return Name;
        }

    }
}
