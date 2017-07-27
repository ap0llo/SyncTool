using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LibGit2Sharp;
using SyncTool.Common.Services;
using SyncTool.FileSystem.Versioning;
using SyncTool.Git.RepositoryAccess;


namespace SyncTool.Git.FileSystem.Versioning
{
    public sealed class GitBasedHistoryService : IHistoryService
    {
        private readonly GitRepository m_Repository;
        private readonly WorkingDirectoryFactory m_WorkingDirectoryFactory;
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
                
                if (!m_Repository.Value.LocalBranchExists(branchName))
                {
                    throw new ItemNotFoundException($"An item named '{name}' was not found");
                }

                return m_HistoryFactory.CreateGitBasedFileSystemHistory(m_Repository.Value, name);
            }
        }

        public IEnumerable<IFileSystemHistory> Items
        {
            get
            {
                return m_Repository.Value.Branches
                    .GetLocalBranchesByPrefix(GitBasedFileSystemHistory.BranchNamePrefix)
                    .Select(b => m_HistoryFactory.CreateGitBasedFileSystemHistory(m_Repository.Value, BranchName.Parse(b.FriendlyName)));
            }
        }


        public GitBasedHistoryService(GitRepository repository, WorkingDirectoryFactory workingDirectoryFactory, [NotNull] GitBasedFileSystemHistoryFactory historyFactory)         
        {
            m_Repository = repository ?? throw new ArgumentNullException(nameof(repository));
            m_WorkingDirectoryFactory = workingDirectoryFactory ?? throw new ArgumentNullException(nameof(workingDirectoryFactory));
            m_HistoryFactory = historyFactory ?? throw new ArgumentNullException(nameof(historyFactory));
        }


        public bool ItemExists(string name) => m_Repository.Value.LocalBranchExists(new BranchName(GitBasedFileSystemHistory.BranchNamePrefix, name));

        public void CreateHistory(string name)
        {
            if (ItemExists(name))
            {
                throw new DuplicateFileSystemHistoryException(name);
            }

            var parentCommitId = m_Repository.Value.Tags[RepositoryInitHelper.InitialCommitTagName].Target.Sha;
            var parentCommit = m_Repository.Value.Lookup<Commit>(parentCommitId);

            var branchName = new BranchName(GitBasedFileSystemHistory.BranchNamePrefix, name);
            m_Repository.Value.CreateBranch(branchName, parentCommit);
        }
    }
}