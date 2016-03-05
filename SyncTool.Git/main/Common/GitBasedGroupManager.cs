// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;
using SyncTool.Common;
using SyncTool.Common.Utilities;
using SyncTool.Git.Configuration.Model;
using SyncTool.Git.Configuration.Reader;
using NativeDirectory = System.IO.Directory;

namespace SyncTool.Git.Common
{
    public class GitBasedGroupManager : IGroupManager
    {
        readonly IDictionary<string, GroupSettings> m_GroupSettings;  
        readonly IRepositoryPathProvider m_PathProvider;
        readonly IGroupSettingsProvider m_SettingsProvider;

        public IEnumerable<string> Groups => m_GroupSettings.Keys;
        
        
       
        public GitBasedGroupManager(IRepositoryPathProvider pathProvider, IGroupSettingsProvider settingsProvider)
        {
            if (pathProvider == null)
            {
                throw new ArgumentNullException(nameof(pathProvider));
            }
            if (settingsProvider == null)
            {
                throw new ArgumentNullException(nameof(settingsProvider));
            }

            m_PathProvider = pathProvider;
            m_SettingsProvider = settingsProvider;

            m_GroupSettings = m_SettingsProvider.GetGroupSettings().ToDictionary(g => g.Name, StringComparer.InvariantCultureIgnoreCase);

        }



        public IGroup GetGroup(string name)
        {
            EnsureGroupExists(name);            
            return new GitBasedGroup(m_GroupSettings[name].Address);          
        }

        public void AddGroup(string name, string address)
        {
            EnsureGroupDoesNotExist(name);
            EnsureAddressDoesNotExist(address);

            var localPath = m_PathProvider.GetRepositoryPath(name);

            // create a transaction for the repository (this will clone the repository)
            var transaction = new GitTransaction(address, localPath);
            try
            {
                transaction.Begin();
            }
            catch (TransactionCloneException ex)
            {
                throw InvalidGroupAddressException.FromAdress(address, ex);
            }
            catch (GitTransactionException ex)
            {
                throw new GroupManagerException("Error adding group", ex);                
            }

            // cloning succeeded, now verify that the local directory is actually a repository for a group           
            if (!RepositoryVerifier.IsValid(transaction.LocalPath))
            {
                throw InvalidGroupAddressException.FromAdress(address);
            }
                       
            transaction.Commit();            

            DoAddGroup(name, address);
        }

        public void CreateGroup(string name, string address)
        {
            EnsureGroupDoesNotExist(name);
            EnsureAddressDoesNotExist(address);

            var directoryPath = m_PathProvider.GetRepositoryPath(name);

            if (NativeDirectory.Exists(directoryPath))
            {
                throw new GroupManagerException($"Cannot create repository for SyncGroup '{name}'. Directory already exists");
            }

            NativeDirectory.CreateDirectory(directoryPath);
            RepositoryInitHelper.InitializeRepository(directoryPath, name);

            using (var repository = new Repository(directoryPath))
            {
                var origin = repository.Network.Remotes.Add("origin", address);

                foreach (var localBranch in repository.GetLocalBranches())
                {
                    repository.Branches.Update(localBranch,
                            b => b.Remote = origin.Name,
                            b => b.UpstreamBranch = localBranch.CanonicalName);

                }
                repository.Network.Push(origin, repository.Branches.GetLocalBranches().ToRefSpecs().Union(repository.Tags.ToRefSpecs()));
            }

            DirectoryHelper.DeleteRecursively(directoryPath);

            DoAddGroup(name, address);
        }

        public void RemoveGroup(string name)
        {            
            EnsureGroupExists(name);
            
            m_GroupSettings.Remove(name);
            m_SettingsProvider.SaveGroupSettings(m_GroupSettings.Values);

            // remove the local directory for the group if it exists
            var directoryPath = m_PathProvider.GetRepositoryPath(name);            
            DirectoryHelper.DeleteRecursively(directoryPath);
        }


        void DoAddGroup(string name, string address)
        {
            m_GroupSettings.Add(name, new GroupSettings { Name = name, Address = address });
            m_SettingsProvider.SaveGroupSettings(m_GroupSettings.Values);
        }

        void EnsureGroupExists(string name)
        {
            if (!m_GroupSettings.ContainsKey(name))
            {
                throw new GroupNotFoundException(name);
            }
        }

        void EnsureGroupDoesNotExist(string name)
        {
            if (m_GroupSettings.ContainsKey(name))
            {
                throw DuplicateGroupException.FromName(name);
            }
        }

        void EnsureAddressDoesNotExist(string address)
        {
            var query = m_GroupSettings.Values.Where(group => group.Address.Equals(address, StringComparison.InvariantCultureIgnoreCase)).ToList();
            if (query.Any())
            {
                throw DuplicateGroupException.FromAddress(address);
            }
        }
    }
}