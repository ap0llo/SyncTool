using System;
using JetBrains.Annotations;
using SyncTool.Common.Services;
using SyncTool.Git.RepositoryAccess;

namespace SyncTool.Git.Common.Services
{
    public class GitBasedService : IService
    {
        protected GitRepository Repository { get; }

        protected WorkingDirectoryFactory WorkingDirectoryFactory { get; }


        public GitBasedService([NotNull] GitRepository group, [NotNull] WorkingDirectoryFactory workingDirectoryFactory)
        {
            WorkingDirectoryFactory = workingDirectoryFactory ?? throw new ArgumentNullException(nameof(workingDirectoryFactory));
            Repository = group ?? throw new ArgumentNullException(nameof(group));
        }        
    }
}