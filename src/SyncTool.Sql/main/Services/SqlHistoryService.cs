using SyncTool.FileSystem.Versioning;
using SyncTool.Sql.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SyncTool.Sql.Services
{
    class SqlHistoryService : AbstractHistoryService
    {
        private readonly IDatabaseContextFactory m_ContextFactory;
        private readonly Func<FileSystemHistoryDo, SqlFileSystemHistory> m_HistoryFactory;

        public override IEnumerable<IFileSystemHistory> Items
        {
            get
            {
                using (var context = m_ContextFactory.CreateContext())
                {
                    return context
                       .FileSystemHistories
                       .ToArray()
                       .Select(historyDo => m_HistoryFactory.Invoke(historyDo));
                }
            }
        }


        public SqlHistoryService(IDatabaseContextFactory contextFactory, Func<FileSystemHistoryDo, SqlFileSystemHistory> historyFactory)
        {
            m_ContextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            m_HistoryFactory = historyFactory ?? throw new ArgumentNullException(nameof(historyFactory));
        }
        

        public override bool ItemExists(string name)
        {
            using (var context = m_ContextFactory.CreateContext())
            {
               return context.FileSystemHistories.Any(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            }
        }


        protected override void DoCreateHistory(string name)
        {
            using (var context = m_ContextFactory.CreateContext())
            {
                var historyDo = new FileSystemHistoryDo(name);
                context.FileSystemHistories.Add(historyDo);
                context.SaveChanges();
            }
        }

        protected override IFileSystemHistory DoGetHistory(string name)
        {
            using (var context = m_ContextFactory.CreateContext())
            {
                var historyDo = context.FileSystemHistories.Single(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                return m_HistoryFactory.Invoke(historyDo);
            }
        }
    }
}
