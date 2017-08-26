using Dapper;
using SyncTool.Sql.Model.Tables;
using System;
using System.Collections.Generic;

namespace SyncTool.Sql.Model
{    
    class FileSystemHistoryRepository
    {
        readonly IDatabaseContextFactory m_ConnectionFactory;


        public IEnumerable<FileSystemHistoryDo> Items
        {
            get
            {
                using (var connection = m_ConnectionFactory.OpenConnection())
                {
                    return connection.Query<FileSystemHistoryDo>(
                        $"SELECT * FROM {FileSystemHistoriesTable.Name}"
                    );
                }
            }
        }


        public FileSystemHistoryRepository(IDatabaseContextFactory connectionFactory)
        {
            m_ConnectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));

            //TODO: Should happen on first access??
            using (var connection = m_ConnectionFactory.OpenConnection())
            {
                FileSystemHistoriesTable.Create(connection);
            }
        }


        public FileSystemHistoryDo GetItemOrDefault(string name)
        {
            using (var connection = m_ConnectionFactory.OpenConnection())
            {
                return connection.QuerySingleOrDefault<FileSystemHistoryDo>($@"
                            SELECT * FROM {FileSystemHistoriesTable.Name}
                            WHERE lower({FileSystemHistoriesTable.Column.Name}) = lower(@name)",
                            new { name }
                        );
            }
        }

        public FileSystemHistoryDo AddItem(FileSystemHistoryDo item)
        {
            using (var connection = m_ConnectionFactory.OpenConnection())
            {
                return connection.QuerySingle<FileSystemHistoryDo>($@"

                    INSERT INTO {FileSystemHistoriesTable.Name} (
                        {FileSystemHistoriesTable.Column.Name}, 
                        {FileSystemHistoriesTable.Column.NormalizedName}, 
                        {FileSystemHistoriesTable.Column.Version}
                    ) 
                    VALUES (
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
}
