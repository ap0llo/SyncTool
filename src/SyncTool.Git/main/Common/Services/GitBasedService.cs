using System;
using SyncTool.Common.Services;
using SyncTool.Git.RepositoryAccess;

namespace SyncTool.Git.Common.Services
{
    public class GitBasedService : IService
    {
        protected GitRepository Repository { get; }


        public GitBasedService(GitRepository group)
        {
            Repository = group ?? throw new ArgumentNullException(nameof(group));
        }        
    }
}