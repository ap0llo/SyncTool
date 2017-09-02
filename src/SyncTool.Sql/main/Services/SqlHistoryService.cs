using SyncTool.FileSystem.Versioning;
using SyncTool.Sql.Model;
using SyncTool.Sql.Model.Tables;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SyncTool.Sql.Services
{
    //TODO: Consider merging configuration service into history service
    class SqlHistoryService : AbstractHistoryService
    {
        readonly FileSystemHistoryRepository m_Repository;
        readonly Func<FileSystemHistoriesTable.Record, SqlFileSystemHistory> m_HistoryFactory;

        public override IEnumerable<IFileSystemHistory> Items => m_Repository.Items.Select(m_HistoryFactory);


        public SqlHistoryService(FileSystemHistoryRepository repository, Func<FileSystemHistoriesTable.Record, SqlFileSystemHistory> historyFactory)
        {
            m_Repository = repository ?? throw new ArgumentNullException(nameof(repository));
            m_HistoryFactory = historyFactory ?? throw new ArgumentNullException(nameof(historyFactory));
        }
        

        public override bool ItemExists(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            return m_Repository.GetItemOrDefault(name) != null;
        }


        protected override void DoCreateHistory(string name) => m_Repository.AddItem(new FileSystemHistoriesTable.Record(name));

        protected override IFileSystemHistory DoGetHistory(string name)
        {
            return m_HistoryFactory.Invoke(m_Repository.GetItemOrDefault(name));            
        }
    }
}
