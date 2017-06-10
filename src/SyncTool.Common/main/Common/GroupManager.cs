using Autofac;
using SyncTool.Common.DI;
using SyncTool.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;


namespace SyncTool.Common
{
    //TODO: Reuse instances/lifetimes in OpenRead()
    class GroupManager : IGroupManager
    {
        readonly IDictionary<string, GroupSettings> m_GroupSettings;  
        readonly IGroupDirectoryPathProvider m_PathProvider;
        readonly IGroupSettingsProvider m_SettingsProvider;
        readonly ILifetimeScope m_ApplicationScope;
        readonly IGroupModuleFactory m_ModuleFactory;

        readonly IDictionary<string, OpenedState> m_OpenedStates = new Dictionary<string, OpenedState>(StringComparer.InvariantCultureIgnoreCase);


        public IEnumerable<string> Groups => m_GroupSettings.Keys;


        public GroupManager(
            IGroupDirectoryPathProvider pathProvider, 
            IGroupSettingsProvider settingsProvider,
            ILifetimeScope applicationScope,
            IGroupModuleFactory moduleFactory)
        {
            m_ApplicationScope = applicationScope ?? throw new ArgumentNullException(nameof(applicationScope));
            m_PathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));
            m_SettingsProvider = settingsProvider ?? throw new ArgumentNullException(nameof(settingsProvider));
            m_ModuleFactory = moduleFactory ?? throw new ArgumentNullException(nameof(moduleFactory));

            m_GroupSettings = m_SettingsProvider.GetGroupSettings().ToDictionary(g => g.Name, StringComparer.InvariantCultureIgnoreCase);
        }


        public IGroup OpenShared(string name)
        {
            EnsureGroupExists(name);

            // update state
            var currentState = m_OpenedStates.GetOrAdd(name, () => new OpenedState());
            if (!currentState.CanOpenShared)
            {
                throw new GroupOpenedException("Group is already opened and cannot be opened for reading");
            }        
            currentState.NotifyOpenedShared();


            var groupSettings = m_GroupSettings[name];
            var groupScope = GetGroupScope(GetGroupStorage(name), groupSettings);

            var group = new Group(groupSettings, groupScope);
            group.Disposed += HandleReadOnlyGroupDisposed;
         
            return group;
        }
        
        public IGroup OpenExclusively(string name)
        {
            EnsureGroupExists(name);

            // update state
            var currentState = m_OpenedStates.GetOrAdd(name, () => new OpenedState());
            if (!currentState.CanOpenExclusively)
            {
                throw new GroupOpenedException("Group is already opened and cannot be opened for writing");
            }
            currentState.NotifyOpenedExclusively();

            var groupSettings = m_GroupSettings[name];
            var groupScope = GetGroupScope(GetGroupStorage(name), groupSettings);

            var group = new Group(groupSettings, groupScope);
            group.Disposed += HandleWritableGroupDisposed;            
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
            m_GroupSettings.Add(name, new GroupSettings(name, address));
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
                builder.RegisterModule(new CommonGroupModule(groupStorage, groupSettings));
                builder.RegisterModule(m_ModuleFactory.CreateModule());
            });
        }
        
        GroupStorage GetGroupStorage(string groupName)
        {
            var path = m_PathProvider.GetGroupDirectoryPath(groupName);
            Directory.CreateDirectory(path);
            return new GroupStorage(path);
        }

        void HandleWritableGroupDisposed(object sender, EventArgs e)
        {
            var group = (Group) sender;
            
            // update state
            m_OpenedStates
                .GetOrAdd(group.Name, () => new OpenedState())
                .NotifyClosedExclusively();
            
            group.LifetimeScope.Dispose();
            group.Disposed -= HandleWritableGroupDisposed;
        }

        void HandleReadOnlyGroupDisposed(object sender, EventArgs e)
        {
            var group = (Group) sender;

            // update state
            m_OpenedStates
                .GetOrAdd(group.Name, () => new OpenedState())
                .NotifyClosedShared();
            
            group.LifetimeScope.Dispose();
            group.Disposed -= HandleReadOnlyGroupDisposed;
        }
    }
}