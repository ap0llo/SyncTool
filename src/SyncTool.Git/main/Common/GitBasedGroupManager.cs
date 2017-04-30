using System;
using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;
using SyncTool.Common;
using SyncTool.Common.Utilities;
using SyncTool.FileSystem;
using SyncTool.Git.Configuration.Model;
using SyncTool.Git.Configuration.Reader;
using NativeDirectory = System.IO.Directory;
using Autofac;

namespace SyncTool.Git.Common
{
    public class GitBasedGroupManager : IGroupManager
    {
        readonly IEqualityComparer<IFileReference> m_FileReferenceComparer; 
        readonly IDictionary<string, GroupSettings> m_GroupSettings;  
        readonly IRepositoryPathProvider m_PathProvider;
        readonly IGroupSettingsProvider m_SettingsProvider;
        readonly ILifetimeScope m_ApplicationScope;

        public IEnumerable<string> Groups => m_GroupSettings.Keys;


        public GitBasedGroupManager(
            IEqualityComparer<IFileReference>  fileReferenceComparer, 
            IRepositoryPathProvider pathProvider, 
            IGroupSettingsProvider settingsProvider,
            ILifetimeScope applicationScope)
        {
            m_ApplicationScope = applicationScope ?? throw new ArgumentNullException(nameof(applicationScope));
            m_FileReferenceComparer = fileReferenceComparer ?? throw new ArgumentNullException(nameof(fileReferenceComparer));
            m_PathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));
            m_SettingsProvider = settingsProvider ?? throw new ArgumentNullException(nameof(settingsProvider));

            m_GroupSettings = m_SettingsProvider.GetGroupSettings().ToDictionary(g => g.Name, StringComparer.InvariantCultureIgnoreCase);
        }



        public IGroup GetGroup(string name)
        {
            EnsureGroupExists(name);

            var groupSettings = m_GroupSettings[name];

            var groupScope = m_ApplicationScope.BeginLifetimeScope(Scope.Group, builder =>
            {
                builder.RegisterInstance(groupSettings).AsSelf().ExternallyOwned();
            });

            var group = groupScope.Resolve<GitBasedGroup>();
            group.Disposed += (s, e) => groupScope.Dispose();
            return group;            
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
            RepositoryInitHelper.InitializeRepository(directoryPath);

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