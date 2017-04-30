using System;
using System.Collections.Generic;
using LibGit2Sharp;
using SyncTool.Common;
using SyncTool.FileSystem;
using SyncTool.Git.FileSystem;
using SyncTool.Git.Configuration.Model;
using Autofac;

namespace SyncTool.Git.Common
{
    public class GitBasedGroup : IGroup
    {
        readonly IEqualityComparer<IFileReference> m_FileReferenceComparer;
        readonly IRepositoryPathProvider m_PathProvider;
        readonly IGitTransaction m_Transaction;
        readonly ILifetimeScope m_GroupScope;

        public event EventHandler Disposed;


        public string Name { get; }        

        public Repository Repository { get; }



        public GitBasedGroup(IEqualityComparer<IFileReference> fileReferenceComparer, IRepositoryPathProvider pathProvider, GroupSettings groupSettings, ILifetimeScope groupScope)
        {
            m_GroupScope = groupScope ?? throw new ArgumentNullException(nameof(groupScope));

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

            m_FileReferenceComparer = fileReferenceComparer ?? throw new ArgumentNullException(nameof(fileReferenceComparer));
            m_PathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));
            Name = name;

            var localRepositoryPath = m_PathProvider.GetRepositoryPath(name);
            m_Transaction = new CachingGitTransaction(repositoryPath, localRepositoryPath);
            m_Transaction.Begin();

            Repository = new Repository(localRepositoryPath);
        }


        public T GetService<T>() where T : IService
        {
            return m_GroupScope.Resolve<T>();

            
            
        }

        public void Dispose()
        {
            Repository.Dispose();
            m_Transaction.Commit();
            Disposed?.Invoke(this, EventArgs.Empty);
        }


        Commit GetConfigurationCommit() => Repository.GetBranch(RepositoryInitHelper.ConfigurationBranchName).Tip;

        internal GitDirectory GetConfigurationRootDirectory() => new GitDirectory(null, "root", GetConfigurationCommit());
    }
}