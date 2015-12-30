// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using LibGit2Sharp;
using SyncTool.Common;
using SyncTool.Configuration.Model;
using SyncTool.FileSystem;
using SyncTool.FileSystem.Versioning;
using SyncTool.Git.Common;
using SyncTool.Git.Configuration;
using SyncTool.Git.FileSystem;
using SyncTool.Git.FileSystem.Versioning;
using SyncTool.Git.Synchronization.Transfer;
using SyncTool.Synchronization.Transfer;

namespace SyncTool.Git.Common
{
    public class GitBasedGroup : IGroup
    {
        protected readonly Repository m_Repository;


        public string Name
        {
            get
            {
                var infoFile = (IReadableFile)GetConfigurationRootDirectory().GetFile(RepositoryInfoFile.RepositoryInfoFileName);
                using (var stream = infoFile.OpenRead())
                {
                    var repositoryInfo = stream.Deserialize<RepositoryInfo>();
                    return repositoryInfo.RepositoryName;
                }
            }
        }

        public Repository Repository => m_Repository;


        public GitBasedGroup(string repositoryPath)
        {
            if (repositoryPath == null)
            {
                throw new ArgumentNullException(nameof(repositoryPath));
            }

            m_Repository = new Repository(repositoryPath);
        }


        public T GetService<T>()
        {
            if (typeof (T) == typeof (IConfigurationService))
            {
                return (T) (object) new GitBasedConfigurationService(this);
            }
            else if (typeof (T) == typeof (IHistoryService))
            {
                return (T) (object) new GitBasedHistoryService(this);
            }
            else if (typeof (T) == typeof (ISynchronizationStateService))
            {
                return (T)(object)new GitSynchronizationStateService(this);
            }
            else
            {
                throw new ServiceNotFoundException(typeof(T));
            }            
        }

        public void Dispose()
        {
            m_Repository.Dispose();
        }


        Commit GetConfigurationCommit() => m_Repository.Branches[RepositoryInitHelper.ConfigurationBranchName].Tip;

        internal GitDirectory GetConfigurationRootDirectory() => new GitDirectory(null, "root", GetConfigurationCommit());
    }
}