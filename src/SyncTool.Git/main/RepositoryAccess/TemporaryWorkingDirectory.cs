using System;
using LibGit2Sharp;
using SyncTool.Utilities;

namespace SyncTool.Git.RepositoryAccess
{
    public sealed class TemporaryWorkingDirectory : IDisposable
    {
        readonly TemporaryDirectory m_TempDirectory;
        readonly Repository m_Repository;
        readonly string m_BranchName;

        /// <summary>
        /// Gets the location of the working directory in the file system
        /// </summary>
        public string Location => m_TempDirectory.FullName;

        /// <summary>
        /// Gets whether there are any changes in the working directory 
        /// </summary>
        public bool HasChanges => m_Repository.RetrieveStatus().IsDirty;

        

        public TemporaryWorkingDirectory(GitOptions options, string sourceUrl, string branchName)
        {
            options = options ?? new GitOptions();
            m_TempDirectory = new TemporaryDirectory(options.TempPath);

            m_BranchName = branchName;

            Repository.Clone(sourceUrl, m_TempDirectory, new CloneOptions {BranchName = branchName, Checkout = true});
            m_Repository = new Repository(m_TempDirectory);
        }


        /// <summary>
        /// Commits all changes in the working directory to the git repository
        /// </summary>
        public string Commit(string commitMessage = "SyncTool Commit")
        {
            Commands.Stage(m_Repository, "*");

            //TODO: this should probably be read from settings
            var signature = new Signature("SyncTool", "SyncTool@example.com", DateTimeOffset.Now);

            var commit = m_Repository.Commit(commitMessage, signature, signature);
            return commit.Sha;
        }

        /// <summary>
        /// Pushes all changes to the repository the temporary working directory was cloned from
        /// </summary>
        public void Push() => m_Repository.Network.Push(m_Repository.Branches[m_BranchName]);

        /// <summary>
        /// Disposes the temporary working directory
        /// This will delete the directory. All Changes that were not commited and pushed will be lost
        /// </summary>
        public void Dispose()
        {
            m_Repository.Dispose();
            m_TempDirectory.Dispose();
        }
             
    }
}