using Grynwald.Utilities.Data;
using System.Data;

namespace SyncTool.Sql.Model
{
    static class FileReferencesTable
    {
        public const string Name = "FileReferences";

        public enum Column
        {
            Id,
            Path,
            LastWriteUnixTimeTicks,
            Length
        }

        public static void Create(IDbConnection connection, DatabaseLimits limits)
        {
            connection.ExecuteNonQuery($@"
                CREATE TABLE {Name} (
                    {Column.Id}                     INTEGER PRIMARY KEY AUTO_INCREMENT,
                    {Column.Path}                   VARCHAR(1000) CHARACTER SET utf8 COLLATE utf8_general_ci,
                    {Column.LastWriteUnixTimeTicks} BIGINT,
                    {Column.Length}                 BIGINT,
                    CONSTRAINT {Name}_Unique UNIQUE (
                        {Column.Path}, 
                        {Column.LastWriteUnixTimeTicks}, 
                        {Column.Length}
                    ));            
            ");
        }
    }
}
