using Dapper;
using SyncTool.Sql.Model.Tables;
using System;
using System.Collections.Generic;

namespace SyncTool.Sql.Model
{
    class SyncFolderRepository
    {
        readonly Database m_Database;


        public IEnumerable<SyncFoldersTable.Record> Items => m_Database.Query<SyncFoldersTable.Record>($"SELECT * FROM {SyncFoldersTable.Name}");


        public SyncFolderRepository(Database database)
        {
            m_Database = database ?? throw new ArgumentNullException(nameof(database));
        }


        public SyncFoldersTable.Record GetItemOrDefault(string name)
        {
            return m_Database.QuerySingleOrDefault<SyncFoldersTable.Record>($@"
                        SELECT * FROM {SyncFoldersTable.Name}
                        WHERE lower({SyncFoldersTable.Column.Name}) = lower(@name)", 
                        new { name }
                    );
        }

        public SyncFoldersTable.Record AddItem(SyncFoldersTable.Record item)
        {
            return m_Database.QuerySingle<SyncFoldersTable.Record>($@"

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

        public void UpdateItem(SyncFoldersTable.Record item)
        {
            using (var connection = m_Database.OpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                var changedRows = connection.ExecuteNonQuery($@"
                    UPDATE {SyncFoldersTable.Name}                    
                    SET {SyncFoldersTable.Column.Path} = @path,
                        {SyncFoldersTable.Column.Version} = @newVersion
                    WHERE {SyncFoldersTable.Column.Version} = @oldVersion AND
                          lower({SyncFoldersTable.Column.Name}) = lower(@name)",

                        ("name", item.Name),
                        ("path", item.Path),
                        ("oldVersion", item.Version),
                        ("newVersion", item.Version + 1)
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
