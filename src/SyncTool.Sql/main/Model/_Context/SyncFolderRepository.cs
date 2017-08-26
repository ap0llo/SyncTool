using Dapper;
using SyncTool.Sql.Model.Tables;
using System;
using System.Collections.Generic;

namespace SyncTool.Sql.Model
{
    class SyncFolderRepository
    {
        readonly IDatabase m_Database;


        public IEnumerable<SyncFolderDo> Items
        {
            get
            {
                using (var connection = m_Database.OpenConnection())
                {
                    return connection.Query<SyncFolderDo>($"SELECT * FROM {SyncFoldersTable.Name}");               
                }                
            }
        }


        public SyncFolderRepository(IDatabase database)
        {
            m_Database = database ?? throw new ArgumentNullException(nameof(database));

            //TODO: Should happen on first access??
            using (var connection = m_Database.OpenConnection())
            {
                SyncFoldersTable.Create(connection);        
            }
        }


        public SyncFolderDo GetItemOrDefault(string name)
        {
            using(var connection = m_Database.OpenConnection())
            {
                return connection.QuerySingleOrDefault<SyncFolderDo>($@"
                            SELECT * FROM {SyncFoldersTable.Name}
                            WHERE lower({SyncFoldersTable.Column.Name}) = lower(@name)", 
                            new { name }
                        );
            }            
        }

        public SyncFolderDo AddItem(SyncFolderDo item)
        {
            using (var connection = m_Database.OpenConnection())
            {
                return connection.QuerySingle<SyncFolderDo>($@"

                    INSERT INTO {SyncFoldersTable.Name} 
                    (
                        {SyncFoldersTable.Column.Name}, 
                        {SyncFoldersTable.Column.Path}, 
                        {SyncFoldersTable.Column.Version}
                    ) 
                    VALUES 
                    (
                        @{nameof(item.Name)}, 
                        @{nameof(item.Path)}, 
                        1
                    );

                    SELECT * FROM {SyncFoldersTable.Name} 
                    WHERE {SyncFoldersTable.Column.Name} = @{nameof(item.Name)} AND 
                          {SyncFoldersTable.Column.Version} = 1;  ", 
                    item
                );                                       
            }
        }

        public void UpdateItem(SyncFolderDo item)
        {
            using (var connection = m_Database.OpenConnection())
            {
                var transaction = connection.BeginTransaction();

                var changedRows = connection.ExecuteNonQuery($@"
                    UPDATE {SyncFoldersTable.Name}                    
                    SET {SyncFoldersTable.Column.Path} = @path,
                        {SyncFoldersTable.Column.Version} = @newVersion
                    WHERE {SyncFoldersTable.Column.Version} = @oldVersion AND
                          lower({SyncFoldersTable.Column.Name}) = lower(@name)",
                          
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
