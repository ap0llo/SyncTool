using System;
using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;
using SyncTool.Common;
using SyncTool.FileSystem.Versioning;
using SyncTool.Git.Common;


namespace SyncTool.Git.FileSystem.Versioning
{
    public sealed class GitBasedHistoryService : GitBasedService, IHistoryService
    {
        



        public IFileSystemHistory this[string name]
        {
            get
            {
                if (String.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentNullException(nameof(name));
                }

                var branchName = new BranchName(GitBasedFileSystemHistory.BranchNamePrefix, name);
                
                if (!GitGroup.Repository.LocalBranchExists(branchName))
                {
                    throw new ItemNotFoundException($"An item named '{name}' was not found");
                }

                return new GitBasedFileSystemHistory(GitGroup.Repository, name);
            }
        }

        public IEnumerable<IFileSystemHistory> Items
        {
            get
            {
                return GitGroup.Repository.Branches
                    .GetLocalBranchesByPrefix(GitBasedFileSystemHistory.BranchNamePrefix)
                    .Select(b => new GitBasedFileSystemHistory(GitGroup.Repository, BranchName.Parse(b.FriendlyName)));
            }
        }



        public GitBasedHistoryService(GitBasedGroup group) : base(group)
        {

        }



        public bool ItemExists(string name) => GitGroup.Repository.LocalBranchExists(new BranchName(GitBasedFileSystemHistory.BranchNamePrefix, name));

        public void CreateHistory(string name)
        {
            var branchName = new BranchName(GitBasedFileSystemHistory.BranchNamePrefix, name);

            if (GitGroup.Repository.LocalBranchExists(branchName))
            {
                throw new DuplicateFileSystemHistoryException(name);
            }

            var parentCommitId = GitGroup.Repository.Tags[RepositoryInitHelper.InitialCommitTagName].Target.Sha;
            var parentCommit = GitGroup.Repository.Lookup<Commit>(parentCommitId);

            GitGroup.Repository.CreateBranch(branchName, parentCommit);
        }


    }
}