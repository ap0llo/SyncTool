using Dapper;
using System;
using System.Collections.Generic;
using static SyncTool.Sql.Model.TypeMapper;

namespace SyncTool.Sql.Model
{
    class SyncFolderRepository
    {
        readonly IDatabaseContextFactory m_ConnectionFactory;


        public IEnumerable<SyncFolderDo> Items
        {
            get
            {
                using (var connection = m_ConnectionFactory.OpenConnection())
                {
                    return connection.Query<SyncFolderDo>($"SELECT * FROM {Table<SyncFolderDo>()}");               
                }                
            }
        }


        public SyncFolderRepository(IDatabaseContextFactory connectionFactory)
        {
            m_ConnectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));

            //TODO: Should happen on first access??
            using (var connection = m_ConnectionFactory.OpenConnection())
            {
                connection.ExecuteNonQuery($@"                
                    CREATE TABLE IF NOT EXISTS {Table<SyncFolderDo>()} (
                        {nameof(SyncFolderDo.Name)} TEXT PRIMARY KEY,
                        {nameof(SyncFolderDo.Path)} TEXT,
                        {nameof(SyncFolderDo.Version)} INTEGER NOT NULL DEFAULT 0)"
                    );                
            }
        }


        public SyncFolderDo GetItemOrDefault(string name)
        {
            using(var connection = m_ConnectionFactory.OpenConnection())
            {
                return connection.QuerySingleOrDefault<SyncFolderDo>($@"
                            SELECT * FROM {Table<SyncFolderDo>()}
                            WHERE lower({nameof(SyncFolderDo.Name)}) = lower(@name)", 
                            new { name }
                        );
            }            
        }

        public SyncFolderDo AddItem(SyncFolderDo item)
        {
            using (var connection = m_ConnectionFactory.OpenConnection())
            {
                return connection.QuerySingle<SyncFolderDo>($@"

                    INSERT INTO {Table<SyncFolderDo>()} (
                        {nameof(SyncFolderDo.Name)}, 
                        {nameof(SyncFolderDo.Path)}, 
                        {nameof(SyncFolderDo.Version)}
                    ) 
                    VALUES (
                        @{nameof(SyncFolderDo.Name)}, 
                        @{nameof(SyncFolderDo.Path)}, 
                        1
                    );

                    SELECT * FROM {Table<SyncFolderDo>()} 
                    WHERE {nameof(SyncFolderDo.Name)} = @{nameof(SyncFolderDo.Name)} AND 
                          {nameof(SyncFolderDo.Version)} = 1;  ", 
                    item
                );                                       
            }
        }

        public void UpdateItem(SyncFolderDo item)
        {
            using (var connection = m_ConnectionFactory.OpenConnection())
            {
                var transaction = connection.BeginTransaction();

                var changedRows = connection.ExecuteNonQuery($@"
                    UPDATE {Table<SyncFolderDo>()}                    
                    SET {nameof(SyncFolderDo.Path)} = @path,
                        {nameof(SyncFolderDo.Version)} = @newVersion
                    WHERE {nameof(SyncFolderDo.Version)} = @oldVersion AND
                          lower({nameof(SyncFolderDo.Name)}) = lower(@name)", 
                        ("name", item.Name),
                        ("path" , item.Path),
                        ("oldVersion", item.Version),
                        ("newVersion" , item.Version + 1)
                );

                if (changedRows == 0)
                    throw new DatabaseUpdateException("No rows affected by update");

                if (changedRows > 1)
                    throw new DatabaseUpdateException("More than one row affected by update");

                transaction.Commit();
            }
        }  
    }
}
