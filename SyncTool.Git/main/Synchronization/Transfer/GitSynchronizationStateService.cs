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
using SyncTool.Git.FileSystem;
using SyncTool.Synchronization.Transfer;

namespace SyncTool.Git.Synchronization.Transfer
{
    public class GitSynchronizationStateService : GitBasedService, ISynchronizationStateService
    {
        const string s_BranchPrefix = "synchronizationState/";


        public IEnumerable<ISynchronizationState> Items
        {
            get
            {
                return GitGroup.Repository.Branches.GetLocalBranches()
                   .Where(b => b.FriendlyName.StartsWith(s_BranchPrefix))
                   .Select(b => new GitSynchronizationState(GitGroup.Repository, b.FriendlyName));
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

                var branchName = s_BranchPrefix + name;
                var branch = GitGroup.Repository.GetLocalBranch(branchName);

                if (branch == null)
                {
                    throw new ItemNotFoundException(name);
                }

                return new GitSynchronizationState(GitGroup.Repository, branchName);
            }
            set
            {
                var branchName = s_BranchPrefix + name;
                var branch = GitGroup.Repository.GetLocalBranch(branchName);

                if (branch == null)
                {
                    var initialCommit = GitGroup.Repository.Lookup<Commit>(GitGroup.Repository.Tags[RepositoryInitHelper.InitialCommitTagName].Target.Sha);
                    branch = GitGroup.Repository.CreateBranch(branchName, initialCommit);
                }

                GitSynchronizationState.Create(GitGroup.Repository, branch.FriendlyName, value);
            }
        }
     

        public GitSynchronizationStateService(GitBasedGroup group) : base(group)
        {
        }


     
    }
}