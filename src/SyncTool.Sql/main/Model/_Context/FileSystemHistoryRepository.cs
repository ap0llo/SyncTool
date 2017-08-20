using Dapper;
using System;
using System.Collections.Generic;

using static SyncTool.Sql.Model.TypeMapper;

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
                        $"SELECT * FROM {Table<FileSystemHistoryDo>()}"
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
                connection.ExecuteNonQuery($@"                
                    CREATE TABLE IF NOT EXISTS {Table<FileSystemHistoryDo>()} (            
                        {nameof(FileSystemHistoryDo.Id)} INTEGER PRIMARY KEY,
                        {nameof(FileSystemHistoryDo.Name)} TEXT NOT NULL,
                        {nameof(FileSystemHistoryDo.NormalizedName)} TEXT UNIQUE NOT NULL,
                        {nameof(FileSystemHistoryDo.Version)} INTEGER NOT NULL DEFAULT 0)"
                    );
            }
        }



        public FileSystemHistoryDo GetItemOrDefault(string name)
        {
            using (var connection = m_ConnectionFactory.OpenConnection())
            {
                return connection.QuerySingleOrDefault<FileSystemHistoryDo>($@"
                            SELECT * FROM {Table<FileSystemHistoryDo>()}
                            WHERE lower({nameof(FileSystemHistoryDo.Name)}) = lower(@name)",
                            new { name }
                        );
            }
        }

        public FileSystemHistoryDo AddItem(FileSystemHistoryDo item)
        {
            using (var connection = m_ConnectionFactory.OpenConnection())
            {
                return connection.QuerySingle<FileSystemHistoryDo>($@"

                    INSERT INTO {Table<FileSystemHistoryDo>()} (
                        {nameof(FileSystemHistoryDo.Name)}, 
                        {nameof(FileSystemHistoryDo.NormalizedName)}, 
                        {nameof(FileSystemHistoryDo.Version)}
                    ) 
                    VALUES (
                        @{nameof(FileSystemHistoryDo.Name)}, 
                        @{nameof(FileSystemHistoryDo.NormalizedName)}, 
                        1
                    );

                    SELECT * FROM {Table<FileSystemHistoryDo>()} 
                    WHERE {nameof(FileSystemHistoryDo.Name)} = @{nameof(FileSystemHistoryDo.Name)} AND 
                          {nameof(FileSystemHistoryDo.Version)} = 1;  ",
                    item
                );
            }
        }
    }
}
