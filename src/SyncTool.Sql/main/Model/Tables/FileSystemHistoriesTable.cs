using JetBrains.Annotations;
using System.Data;

namespace SyncTool.Sql.Model.Tables
{
    public static class FileSystemHistoriesTable
    {
        public const string Name = "FileSystemHistories";

        public enum Column
        {
            Id,
            Name,
            NormalizedName,
            Version
        }

        public class Record
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public string NormalizedName { get; set; }

            public int Version { get; set; }

            [UsedImplicitly]
            public Record() { }

            public Record(string name)
            {
                Name = name;
                NormalizedName = name.NormalizeCaseInvariant();
            }
        }

        public static void Create(IDbConnection connection)
        {
            connection.ExecuteNonQuery($@"                
                CREATE TABLE {Name} (            
                    {Column.Id}             INTEGER PRIMARY KEY,
                    {Column.Name}           TEXT NOT NULL,
                    {Column.NormalizedName} TEXT UNIQUE NOT NULL,
                    {Column.Version}        INTEGER NOT NULL DEFAULT 0)
            ");
        }
    }
}
