// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;
using SyncTool.Common;
using SyncTool.FileSystem.Git;
using SyncTool.FileSystem.Git.Utilities;

namespace SyncTool.Synchronization.Transfer
{
    public class GitSynchronizationStateGroup : GitBasedGroup, ISynchronizationStateGroup
    {
        const string s_BranchPrefix = "synchronizationState/";


        public IEnumerable<ISynchronizationState> Items
        {
            get
            {
                return m_Repository.Branches.GetLocalBranches()
                   .Where(b => b.FriendlyName.StartsWith(s_BranchPrefix))
                   .Select(b => new GitSynchronizationState(m_Repository, b.FriendlyName));
            }
        }


        public GitSynchronizationStateGroup(string repositoryPath) : base(repositoryPath)
        {
        }


        public ISynchronizationState GetItem(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var branchName = s_BranchPrefix + name;
            var branch = m_Repository.GetLocalBranch(branchName);

            if (branch == null)
            {
                throw new ItemNotFoundException(name);
            }

            return new GitSynchronizationState(m_Repository, branchName);
        }

        public void SetState(string name, ISynchronizationState state)
        {
            var branchName = s_BranchPrefix + name;
            var branch = m_Repository.GetLocalBranch(branchName);

            if (branch == null)
            {
                var initialCommit = m_Repository.Lookup<Commit>(m_Repository.Tags[RepositoryInitHelper.InitialCommitTagName].Target.Sha);
                branch = m_Repository.CreateBranch(branchName, initialCommit);
            }

            GitSynchronizationState.Create(m_Repository, branch.FriendlyName, state);
        }
    }
}