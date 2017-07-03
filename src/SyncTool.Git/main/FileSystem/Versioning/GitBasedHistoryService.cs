using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LibGit2Sharp;
using SyncTool.Common.Services;
using SyncTool.FileSystem.Versioning;
using SyncTool.Git.Common;
using SyncTool.Git.Common.Services;
using SyncTool.Git.RepositoryAccess;


namespace SyncTool.Git.FileSystem.Versioning
{
    public sealed class GitBasedHistoryService : GitBasedService, IHistoryService
    {
        readonly GitBasedFileSystemHistoryFactory m_HistoryFactory;

        public IFileSystemHistory this[string name]
        {
            get
            {
                if (String.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentNullException(nameof(name));
                }

                var branchName = new BranchName(GitBasedFileSystemHistory.BranchNamePrefix, name);
                
                if (!Repository.Value.LocalBranchExists(branchName))
                {
                    throw new ItemNotFoundException($"An item named '{name}' was not found");
                }

                return m_HistoryFactory.CreateGitBasedFileSystemHistory(Repository.Value, name);
            }
        }

        public IEnumerable<IFileSystemHistory> Items
        {
            get
            {
                return Repository.Value.Branches
                    .GetLocalBranchesByPrefix(GitBasedFileSystemHistory.BranchNamePrefix)
                    .Select(b => m_HistoryFactory.CreateGitBasedFileSystemHistory(Repository.Value, BranchName.Parse(b.FriendlyName)));
            }
        }


        public GitBasedHistoryService(GitRepository repository, WorkingDirectoryFactory workingDirectoryFactory, [NotNull] GitBasedFileSystemHistoryFactory historyFactory) 
            : base(repository, workingDirectoryFactory)
        {
            m_HistoryFactory = historyFactory ?? throw new ArgumentNullException(nameof(historyFactory));
        }


        public bool ItemExists(string name) => Repository.Value.LocalBranchExists(new BranchName(GitBasedFileSystemHistory.BranchNamePrefix, name));

        public void CreateHistory(string name)
        {
            var branchName = new BranchName(GitBasedFileSystemHistory.BranchNamePrefix, name);

            if (Repository.Value.LocalBranchExists(branchName))
            {
                throw new DuplicateFileSystemHistoryException(name);
            }

            var parentCommitId = Repository.Value.Tags[RepositoryInitHelper.InitialCommitTagName].Target.Sha;
            var parentCommit = Repository.Value.Lookup<Commit>(parentCommitId);

            Repository.Value.CreateBranch(branchName, parentCommit);
        }
    }
}