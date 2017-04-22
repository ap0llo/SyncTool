using System;
using SyncTool.Common;

namespace SyncTool.Git.Common
{
    public class GitBasedService : IService
    {
                
        protected GitBasedGroup GitGroup { get; }


        public GitBasedService(GitBasedGroup group)
        {
            if (group == null)
            {
                throw new ArgumentNullException(nameof(group));
            }
            GitGroup = group;
        }




    }
}