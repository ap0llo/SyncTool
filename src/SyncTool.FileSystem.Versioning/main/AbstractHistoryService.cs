using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SyncTool.Common.Services;

namespace SyncTool.FileSystem.Versioning
{
    public abstract class AbstractHistoryService : IHistoryService
    {
        readonly ILogger<AbstractHistoryService> m_Logger;

        public IFileSystemHistory this[string name]
        {
            get
            {
                if (String.IsNullOrWhiteSpace(name))
                    throw new ArgumentNullException(nameof(name));

                if (!ItemExists(name))
                    throw new ItemNotFoundException($"An filesystem history named '{name}' was not found");

                return DoGetHistory(name);
            }
        }

        public abstract IEnumerable<IFileSystemHistory> Items { get; }


        public AbstractHistoryService(ILogger<AbstractHistoryService> logger) =>
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        

        public void CreateHistory(string name)
        {
            m_Logger.LogDebug($"Creating history '{name}'");

            if (ItemExists(name))
                throw new DuplicateFileSystemHistoryException(name);

            DoCreateHistory(name);
        }


        public abstract bool ItemExists(string name);

        protected abstract IFileSystemHistory DoGetHistory(string name);

        protected abstract void DoCreateHistory(string name);
    }
}
