// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;
using SyncTool.Common;
using SyncTool.Git.Common;
using SyncTool.Synchronization.Transfer;

namespace SyncTool.Git.Synchronization.Transfer
{
    public class GitSynchronizationStateService : GitBasedService, ISynchronizationStateService
    {
        


        public IEnumerable<ISynchronizationState> Items
        {
            get
            {
                return GitGroup.Repository.Branches
                    .GetLocalBranchesByPrefix(GitSynchronizationState.BranchNamePrefix)
                    .Select(b => new GitSynchronizationState(GitGroup.Repository, BranchName.Parse(b.FriendlyName)));
            }
        }

        public ISynchronizationState this[string name]
        {
            get
            {
                if (String.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentNullException(nameof(name));
                }

                var branchName = new BranchName(GitSynchronizationState.BranchNamePrefix, name);
                var branch = GitGroup.Repository.GetLocalBranch(branchName);

                if (branch == null)
                {
                    throw new ItemNotFoundException(name);
                }

                return new GitSynchronizationState(GitGroup.Repository, BranchName.Parse(branch.FriendlyName));
            }
            set
            {
                var branchName = new BranchName(GitSynchronizationState.BranchNamePrefix, name);
                var branch = GitGroup.Repository.GetLocalBranch(branchName);

                if (branch == null)
                {
                    var initialCommit = GitGroup.Repository.Lookup<Commit>(GitGroup.Repository.GetInitialCommit().Sha);
                    branch = GitGroup.Repository.CreateBranch(branchName, initialCommit);
                }

                GitSynchronizationState.Create(GitGroup.Repository, BranchName.Parse(branch.FriendlyName), value);
            }
        }
     

        public GitSynchronizationStateService(GitBasedGroup group) : base(group)
        {
        }


        public bool ItemExists(string name) => GitGroup.Repository.LocalBranchExists(new BranchName(GitSynchronizationState.BranchNamePrefix, name));
    }
}