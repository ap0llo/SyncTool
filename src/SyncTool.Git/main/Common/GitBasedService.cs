using System;
using SyncTool.Common;

namespace SyncTool.Git.Common
{
    public class GitBasedService : IService
    {
                
        protected GitBasedGroup GitGroup { get; }


        public GitBasedService(GitBasedGroup group)
        {
            GitGroup = group ?? throw new ArgumentNullException(nameof(group));
        }




    }
}