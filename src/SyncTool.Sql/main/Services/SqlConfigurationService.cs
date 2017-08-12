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
        private readonly SyncFolderRepository m_Repository;

        public override IEnumerable<SyncFolder> Items
        {
            get
            {
                return m_Repository.Items.Select(x => x.ToSyncFolder()).ToArray();                
            }
        }

        public SqlConfigurationService(SyncFolderRepository repository)
        {
            m_Repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }


        public override bool ItemExists(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            return m_Repository.GetItemOrDefault(name) != null;            
        }

        protected override SyncFolder GetItemOrDefault(string name)
        {
            return m_Repository.GetItemOrDefault(name)?.ToSyncFolder();            
        }

        protected override void DoAddItem(SyncFolder folder)
        {
            m_Repository.AddItem(SyncFolderDo.FromSyncFolder(folder));
        }       

        protected override void DoUpdateItem(SyncFolder folder)
        {
            var existingItem = m_Repository.GetItemOrDefault(folder.Name);
            existingItem.Path = folder.Path;

            m_Repository.UpdateItem(existingItem);                   
        }
    }
}
