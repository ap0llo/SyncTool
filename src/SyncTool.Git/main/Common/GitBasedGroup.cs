using System;
using System.Collections.Generic;
using LibGit2Sharp;
using SyncTool.Common;
using SyncTool.Configuration.Model;
using SyncTool.FileSystem;
using SyncTool.FileSystem.Versioning;
using SyncTool.Git.Configuration;
using SyncTool.Git.FileSystem;
using SyncTool.Git.FileSystem.Versioning;
using SyncTool.Git.Synchronization.Conflicts;
using SyncTool.Git.Synchronization.State;
using SyncTool.Git.Synchronization.SyncActions;
using SyncTool.Synchronization.ChangeGraph;
using SyncTool.Synchronization.Conflicts;
using SyncTool.Synchronization.State;
using SyncTool.Synchronization.SyncActions;

namespace SyncTool.Git.Common
{
    public class GitBasedGroup : IGroup
    {
        readonly IEqualityComparer<IFileReference> m_FileReferenceComparer;
        readonly IRepositoryPathProvider m_PathProvider;
        readonly IGitTransaction m_Transaction;

        public string Name { get; }        

        public Repository Repository { get; }

        public GitBasedGroup(IEqualityComparer<IFileReference> fileReferenceComparer, IRepositoryPathProvider pathProvider, string name, string repositoryPath)
        {
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
            if (typeof (T) == typeof (IConfigurationService))
            {
                return (T) (object) new GitBasedConfigurationService(this);
            }
            else if (typeof (T) == typeof (IHistoryService))
            {
                return (T) (object) new GitBasedHistoryService(this);
            }
            else if (typeof (T) == typeof (ISyncPointService))
            {
                return (T)(object)new GitSyncPointService(this);
            }
            else if (typeof(T) == typeof(IConflictService))
            {
                return (T) (object) new GitConflictService(this);
            }
            else if (typeof(T) == typeof(ISyncActionService))
            {
                return (T)(object)new GitSyncActionService(this);
            }
            else if (typeof(T) == typeof(IMultiFileSystemHistoryService))
            {
                return (T)(object)new GitBasedMultiFileSystemHistoryService(this, GetService<IHistoryService>());
            }
            else
            {
                throw new ServiceNotFoundException(typeof(T));
            }            
        }

        public void Dispose()
        {
            Repository.Dispose();
            m_Transaction.Commit();
        }


        Commit GetConfigurationCommit() => Repository.GetBranch(RepositoryInitHelper.ConfigurationBranchName).Tip;

        internal GitDirectory GetConfigurationRootDirectory() => new GitDirectory(null, "root", GetConfigurationCommit());
    }
}