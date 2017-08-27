using Dapper;
using SyncTool.Sql.Model.Tables;
using System;
using System.Collections.Generic;

namespace SyncTool.Sql.Model
{    
    class FileSystemHistoryRepository
    {
        readonly IDatabase m_Database;


        public IEnumerable<FileSystemHistoryDo> Items => m_Database.Query<FileSystemHistoryDo>($"SELECT * FROM {FileSystemHistoriesTable.Name}");


        public FileSystemHistoryRepository(IDatabase database)
        {
            m_Database = database ?? throw new ArgumentNullException(nameof(database));

            //TODO: Should happen on first access??
            using (var connection = m_Database.OpenConnection())
            {
                FileSystemHistoriesTable.Create(connection);
            }
        }


        public FileSystemHistoryDo GetItemOrDefault(string name)
        {
            return m_Database.QuerySingleOrDefault<FileSystemHistoryDo>($@"
                        SELECT * FROM {FileSystemHistoriesTable.Name}
                        WHERE lower({FileSystemHistoriesTable.Column.Name}) = lower(@name)",
                        new { name }
                    );
        }

        public FileSystemHistoryDo AddItem(FileSystemHistoryDo item)
        {
            return m_Database.QuerySingle<FileSystemHistoryDo>($@"

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
