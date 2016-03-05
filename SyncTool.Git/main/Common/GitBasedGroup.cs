// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using LibGit2Sharp;
using SyncTool.Common;
using SyncTool.Configuration.Model;
using SyncTool.FileSystem.Versioning;
using SyncTool.Git.Configuration;
using SyncTool.Git.FileSystem;
using SyncTool.Git.FileSystem.Versioning;
using SyncTool.Git.Synchronization.Transfer;
using SyncTool.Synchronization.Transfer;

namespace SyncTool.Git.Common
{
    public class GitBasedGroup : IGroup
    {
        public string Name { get; }        

        public Repository Repository { get; }

        //TODO: use GitTransaction so repositoryPath can be a remote repository
        public GitBasedGroup(string name, string repositoryPath)
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

            Name = name;
            Repository = new Repository(repositoryPath);
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
            else if (typeof (T) == typeof (ISynchronizationStateService))
            {
                return (T)(object)new GitSynchronizationStateService(this);
            }
            else
            {
                throw new ServiceNotFoundException(typeof(T));
            }            
        }

        public void Dispose() => Repository.Dispose();


        Commit GetConfigurationCommit() => Repository.Branches[RepositoryInitHelper.ConfigurationBranchName].Tip;

        internal GitDirectory GetConfigurationRootDirectory() => new GitDirectory(null, "root", GetConfigurationCommit());
    }
}