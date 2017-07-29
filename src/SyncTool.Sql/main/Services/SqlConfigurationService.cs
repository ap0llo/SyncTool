using SyncTool.Configuration;
using SyncTool.Sql.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SyncTool.Sql.Services
{
    class SqlConfigurationService : AbstractConfigurationService, IConfigurationService
    {
        private readonly IDatabaseContextFactory m_ContextFactory;

        public override IEnumerable<SyncFolder> Items
        {
            get
            {
                using (var context = m_ContextFactory.CreateContext())
                {
                    return context.SyncFolders.Select(x => x.ToSyncFolder()).ToArray();
                }
            }
        }

        public SqlConfigurationService(IDatabaseContextFactory contextFactory)
        {
            m_ContextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));            
        }


        public override bool ItemExists(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            using (var context = m_ContextFactory.CreateContext())
            {
                return context.SyncFolders.Any(f => f.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            }
        }

        protected override SyncFolder GetItemOrDefault(string name)
        {
            using (var context = m_ContextFactory.CreateContext())
            {
                return context.SyncFolders.SingleOrDefault(f => f.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))?.ToSyncFolder();
            }
        }

        protected override void DoAddItem(SyncFolder folder)
        {
            using (var context = m_ContextFactory.CreateContext())
            {
                context.Add(SyncFolderDo.FromSyncFolder(folder));
                context.SaveChanges();
            }
        }       

        protected override void DoUpdateItem(SyncFolder folder)
        {
            using (var context = m_ContextFactory.CreateContext())
            {
                var existingItem = context.SyncFolders.Single(x => x.Name.Equals(folder.Name, StringComparison.InvariantCultureIgnoreCase));
                existingItem.Path = folder.Path;

                context.Update(existingItem);            
            }
        }
    }
}
