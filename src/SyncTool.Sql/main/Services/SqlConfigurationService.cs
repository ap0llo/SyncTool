using Microsoft.Extensions.Logging;
using SyncTool.Configuration;
using SyncTool.Sql.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SyncTool.Sql.Services
{
    class SqlConfigurationService : AbstractConfigurationService
    {        
        readonly SyncFolderRepository m_Repository;
        readonly ILogger<SqlConfigurationService> m_Logger;

        public override IEnumerable<SyncFolder> Items => m_Repository.Items.Select(x => x.ToSyncFolder()).ToArray();


        public SqlConfigurationService(SyncFolderRepository repository, ILogger<SqlConfigurationService> logger)
        {
            m_Repository = repository ?? throw new ArgumentNullException(nameof(repository));
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public override bool ItemExists(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            return m_Repository.GetItemOrDefault(name) != null;            
        }

        protected override SyncFolder GetItemOrDefault(string name) => m_Repository.GetItemOrDefault(name)?.ToSyncFolder();

        protected override void DoAddItem(SyncFolder folder)
        {
            m_Logger.LogDebug($"Adding item '{folder.Name}', Path = {folder.Path}");
            m_Repository.AddItem(SyncFolderDo.FromSyncFolder(folder));
        }

        protected override void DoUpdateItem(SyncFolder folder)
        {
            m_Logger.LogDebug($"Adding item '{folder.Name}', Path = {folder.Path}");

            var existingItem = m_Repository.GetItemOrDefault(folder.Name);
            existingItem.Path = folder.Path;

            m_Repository.UpdateItem(existingItem);                   
        }
    }
}
