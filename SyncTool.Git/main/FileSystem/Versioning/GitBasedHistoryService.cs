// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;
using SyncTool.Common;
using SyncTool.FileSystem.Versioning;
using SyncTool.Git.Common;
using NativeDirectory = System.IO.Directory;


namespace SyncTool.Git.FileSystem.Versioning
{
    public sealed class GitBasedHistoryService : GitBasedService, IHistoryService
    {
        const string s_BranchPrefix = "filesystemhistory/";



        public IFileSystemHistory this[string name]
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

                return new GitBasedFileSystemHistory(GitGroup.Repository, branch.FriendlyName);
            }
        }

        public IEnumerable<IFileSystemHistory> Items
        {
            get
            {
                return GitGroup.Repository.Branches.GetLocalBranches()
                 .Where(b => b.FriendlyName.StartsWith(s_BranchPrefix))
                 .Select(b => new GitBasedFileSystemHistory(GitGroup.Repository, b.FriendlyName));
            }
        }



        public GitBasedHistoryService(GitBasedGroup group) : base(group)
        {

        }



        public bool ItemExists(string name) => GitGroup.Repository.LocalBranchExists(s_BranchPrefix + name);

        public void CreateHistory(string name)
        {
            var branchName = s_BranchPrefix + name;
            var parentCommitId = GitGroup.Repository.Tags[RepositoryInitHelper.InitialCommitTagName].Target.Sha;
            var parentCommit = GitGroup.Repository.Lookup<Commit>(parentCommitId);

            GitGroup.Repository.CreateBranch(branchName, parentCommit);
        }


    }
}