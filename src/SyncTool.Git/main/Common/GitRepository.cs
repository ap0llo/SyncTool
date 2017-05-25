using LibGit2Sharp;
using SyncTool.Common;
using SyncTool.Git.FileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncTool.Git.Common
{
    public class GitRepository : IDisposable
    {
        readonly IGroupDirectoryPathProvider m_PathProvider;
        readonly IGitTransaction m_Transaction;


        public Repository Value { get; }


        public GitRepository(IGroupDirectoryPathProvider pathProvider, GroupSettings groupSettings)
        {
            if (groupSettings == null)
            {
                throw new ArgumentNullException(nameof(groupSettings));
            }

            //TODO: Remove checks once they have been implemented in GroupSettings
            var name = groupSettings.Name;
            var repositoryPath = groupSettings.Address;

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (String.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name must not be empty", nameof(name));
            }

            if (repositoryPath == null)
            {
                throw new ArgumentNullException(nameof(repositoryPath));
            }

            m_PathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));
            var localRepositoryPath = m_PathProvider.GetGroupDirectoryPath(name);
            m_Transaction = new CachingGitTransaction(repositoryPath, localRepositoryPath);
            m_Transaction.Begin();

            Value = new Repository(localRepositoryPath);
        }

        public void Dispose()
        {
            m_Transaction.Commit();
            Value.Dispose();
        }


        Commit GetConfigurationCommit() => Value.GetBranch(RepositoryInitHelper.ConfigurationBranchName).Tip;

        internal GitDirectory GetConfigurationRootDirectory() => new GitDirectory(null, "root", GetConfigurationCommit());
    }
}
