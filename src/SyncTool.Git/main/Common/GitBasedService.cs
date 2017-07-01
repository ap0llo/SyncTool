using System;
using SyncTool.Common.Services;

namespace SyncTool.Git.Common
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