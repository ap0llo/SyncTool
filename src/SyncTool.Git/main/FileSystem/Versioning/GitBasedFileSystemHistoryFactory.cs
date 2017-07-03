using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LibGit2Sharp;
using SyncTool.Git.RepositoryAccess;
using SyncTool.Utilities;

namespace SyncTool.Git.FileSystem.Versioning
{
    public class GitBasedFileSystemHistoryFactory
    {
        readonly WorkingDirectoryFactory m_WorkingDirectoryFactory;
        readonly IDictionary<(Repository, BranchName), GitBasedFileSystemHistory> m_Instances = new Dictionary<(Repository, BranchName), GitBasedFileSystemHistory>();


        public GitBasedFileSystemHistoryFactory([NotNull] WorkingDirectoryFactory workingDirectoryFactory)
        {
            m_WorkingDirectoryFactory = workingDirectoryFactory ?? throw new ArgumentNullException(nameof(workingDirectoryFactory));
        }

        public GitBasedFileSystemHistory CreateGitBasedFileSystemHistory(Repository repository, BranchName branchName) 
            => m_Instances.GetOrAdd((repository, branchName), () => new GitBasedFileSystemHistory(m_WorkingDirectoryFactory, repository, branchName));

        public GitBasedFileSystemHistory CreateGitBasedFileSystemHistory(Repository repository, string historyName) 
            => CreateGitBasedFileSystemHistory(repository, new BranchName(GitBasedFileSystemHistory.BranchNamePrefix, historyName));
    }
}