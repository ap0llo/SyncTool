using System;
using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;
using SyncTool.Common;
using SyncTool.Common.Utilities;
using SyncTool.FileSystem;
using SyncTool.Git.Configuration.Reader;
using Autofac;
using NativeDirectory = System.IO.Directory;

namespace SyncTool.Git.Common
{
    public class GitBasedGroupManager : IGroupManager
    {
        readonly IEqualityComparer<IFileReference> m_FileReferenceComparer; 
        readonly IDictionary<string, GroupSettings> m_GroupSettings;  
        readonly IGroupDirectoryPathProvider m_PathProvider;
        readonly IGroupSettingsProvider m_SettingsProvider;
        readonly ILifetimeScope m_ApplicationScope;


        public IEnumerable<string> Groups => m_GroupSettings.Keys;


        public GitBasedGroupManager(
            IEqualityComparer<IFileReference>  fileReferenceComparer, 
            IGroupDirectoryPathProvider pathProvider, 
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

            var groupScope = GetGroupScope(GetGroupStorage(name), groupSettings);

            var group = groupScope.Resolve<Group>();
            group.Disposed += (s, e) => groupScope.Dispose();
            return group;            
        }

        public void AddGroup(string name, string address)
        {
            EnsureGroupDoesNotExist(name);
            EnsureAddressDoesNotExist(address);

            using (var groupScope = GetGroupScope(GetGroupStorage(name)))
            {
                var validator = groupScope.Resolve<IGroupValidator>();
                try
                {
                    validator.EnsureGroupIsValid(name, address);
                }
                catch (ValidationException ex)
                {
                    throw new GroupManagerException($"Cannot add group '{name}'", ex);
                }
            }
            
            DoAddGroup(name, address);
        }

        public void CreateGroup(string name, string address)
        {
            EnsureGroupDoesNotExist(name);
            EnsureAddressDoesNotExist(address);

            using (var scope = GetGroupScope(GetGroupStorage(name)))
            {
                var initializer = scope.Resolve<IGroupInitializer>();

                try
                {
                    initializer.Initialize(name, address);
                }
                catch (InitializationException ex)
                {
                    throw new GroupManagerException("Error during group initilaization", ex);
                }
            }

            DoAddGroup(name, address);
        }

        public void RemoveGroup(string name)
        {            
            EnsureGroupExists(name);
            
            m_GroupSettings.Remove(name);
            m_SettingsProvider.SaveGroupSettings(m_GroupSettings.Values);

            // remove the local directory for the group if it exists
            var directoryPath = m_PathProvider.GetGroupDirectoryPath(name);            
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

        ILifetimeScope GetGroupScope(GroupStorage groupStorage, GroupSettings groupSettings = null)
        {
            return m_ApplicationScope.BeginLifetimeScope(Scope.Group, builder =>
            {
                if (groupSettings != null)
                {
                    builder.RegisterInstance(groupSettings).AsSelf().ExternallyOwned();
                }

                builder.RegisterInstance(groupStorage).AsSelf();
                
            });
        }


        GroupStorage GetGroupStorage(string groupName)
        {
            var path = m_PathProvider.GetGroupDirectoryPath(groupName);
            NativeDirectory.CreateDirectory(path);
            return new GroupStorage(path);
        }

    }
}