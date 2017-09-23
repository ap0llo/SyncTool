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
    public sealed class GitBasedHistoryService : AbstractHistoryService, IHistoryService
    {
        private readonly GitRepository m_Repository;
        private readonly WorkingDirectoryFactory m_WorkingDirectoryFactory;
        readonly GitBasedFileSystemHistoryFactory m_HistoryFactory;


        public override IEnumerable<IFileSystemHistory> Items
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


        public override bool ItemExists(string name) => m_Repository.Value.LocalBranchExists(new BranchName(GitBasedFileSystemHistory.BranchNamePrefix, name));

        protected override void DoCreateHistory(string name)
        {            
            var parentCommitId = m_Repository.Value.Tags[RepositoryInitHelper.InitialCommitTagName].Target.Sha;
            var parentCommit = m_Repository.Value.Lookup<Commit>(parentCommitId);

            var branchName = new BranchName(GitBasedFileSystemHistory.BranchNamePrefix, name);
            m_Repository.Value.CreateBranch(branchName, parentCommit);
        }

        protected override IFileSystemHistory DoGetHistory(string name)
        {
            var branchName = new BranchName(GitBasedFileSystemHistory.BranchNamePrefix, name);
            return m_HistoryFactory.CreateGitBasedFileSystemHistory(m_Repository.Value, name);
        }
    }
}