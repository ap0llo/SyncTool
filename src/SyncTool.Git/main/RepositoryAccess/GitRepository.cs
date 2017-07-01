using LibGit2Sharp;
using SyncTool.Common.Groups;
using SyncTool.Git.FileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SyncTool.Git.RepositoryAccess.Transactions;

namespace SyncTool.Git.RepositoryAccess
{
    public class GitRepository : IDisposable
    {
        readonly GroupStorage m_GroupStorage;
        readonly IGitTransaction m_Transaction;


        public Repository Value { get; }


        public GitRepository(GroupStorage groupStorage, GroupSettings groupSettings)
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

            m_GroupStorage = groupStorage ?? throw new ArgumentNullException(nameof(groupStorage));            
            m_Transaction = new CachingGitTransaction(repositoryPath, groupStorage.Path);
            m_Transaction.Begin();

            Value = new Repository(groupStorage.Path);
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
