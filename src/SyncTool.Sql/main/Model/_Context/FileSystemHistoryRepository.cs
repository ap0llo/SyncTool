using Dapper;
using SyncTool.Sql.Model.Tables;
using System;
using System.Collections.Generic;

namespace SyncTool.Sql.Model
{    
    class FileSystemHistoryRepository
    {
        readonly Database m_Database;


        public IEnumerable<FileSystemHistoriesTable.Record> Items => m_Database.Query<FileSystemHistoriesTable.Record>($"SELECT * FROM {FileSystemHistoriesTable.Name}");


        public FileSystemHistoryRepository(Database database)
        {
            m_Database = database ?? throw new ArgumentNullException(nameof(database));
        }


        public FileSystemHistoriesTable.Record GetItemOrDefault(string name)
        {
            return m_Database.QuerySingleOrDefault<FileSystemHistoriesTable.Record>($@"
                        SELECT * FROM {FileSystemHistoriesTable.Name}
                        WHERE lower({FileSystemHistoriesTable.Column.Name}) = lower(@name)",
                        new { name }
                    );
        }

        public FileSystemHistoriesTable.Record AddItem(FileSystemHistoriesTable.Record item)
        {
            return m_Database.QuerySingle<FileSystemHistoriesTable.Record>($@"

                INSERT INTO {FileSystemHistoriesTable.Name} 
                (
                    {FileSystemHistoriesTable.Column.Name}, 
                    {FileSystemHistoriesTable.Column.NormalizedName}, 
                    {FileSystemHistoriesTable.Column.Version}
                ) 
                VALUES 
                (
                    @{nameof(item.Name)}, 
                    @{nameof(item.NormalizedName)}, 
                    1
                );

                SELECT * FROM {FileSystemHistoriesTable.Name} 
                WHERE {FileSystemHistoriesTable.Column.Name} = @{nameof(item.Name)} AND 
                        {FileSystemHistoriesTable.Column.Version} = 1;  ",
                item
            );
        }
    }
}
