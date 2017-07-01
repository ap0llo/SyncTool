using System;
using System.Collections.Generic;
using LibGit2Sharp;
using SyncTool.Git.RepositoryAccess;
using SyncTool.Utilities;

namespace SyncTool.Git.FileSystem.Versioning
{
    public class GitBasedFileSystemHistoryFactory
    {
        readonly IDictionary<(Repository, BranchName), GitBasedFileSystemHistory> m_Instances = new Dictionary<(Repository, BranchName), GitBasedFileSystemHistory>();


        public GitBasedFileSystemHistory CreateGitBasedFileSystemHistory(Repository repository, BranchName branchName)
        {
            return m_Instances.GetOrAdd((repository, branchName), () => new GitBasedFileSystemHistory(repository, branchName));
        }

        public GitBasedFileSystemHistory CreateGitBasedFileSystemHistory(Repository repository, string historyName) 
            => CreateGitBasedFileSystemHistory(repository, new BranchName(GitBasedFileSystemHistory.BranchNamePrefix, historyName));
    }
}