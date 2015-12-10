// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using SyncTool.Configuration.Model;
using SyncTool.FileSystem;
using SyncTool.FileSystem.Git;
using SyncTool.Utilities;
using NativeDirectory = System.IO.Directory;

namespace SyncTool.Configuration.Git
{
    public sealed class GitBasedSyncGroupManager : GitBasedGroupManager, ISyncGroupManager
    {
        

        public IEnumerable<string> SyncGroups => Groups;
        

        public ISyncGroup GetSyncGroup(string name)
        {
            try
            {
                return new GitBasedSyncGroup(GetRepositoryPath(name));
            }
            catch (GroupNotFoundException ex)
            {                
                throw new SyncGroupNotFoundException(name, ex);
            }
        }


        public GitBasedSyncGroupManager(IRepositoryPathProvider pathProvider) : base(pathProvider)
        {            
        }


        //TODO: Move to base class
        public void AddSyncGroup(string name)
        {
            if (SyncGroups.Contains(name, StringComparer.CurrentCultureIgnoreCase))
            {
                throw new DuplicateSyncGroupException(name);
            }

            var directoryPath = m_PathProvider.GetRepositoryPath(name);

            if (NativeDirectory.Exists(directoryPath))
            {
                throw new ConfigurationException($"Cannot create repository for SyncGroup '{name}'. Directory already exists");
            }

            NativeDirectory.CreateDirectory(directoryPath);
            RepositoryInitHelper.InitializeRepository(directoryPath, name);            
        }

        public void RemoveSyncGroup(string name)
        {
            if (SyncGroups.Contains(name, StringComparer.InvariantCultureIgnoreCase) == false)
            {
                throw new SyncGroupNotFoundException(name);
            }

            var directoryPath = m_PathProvider.GetRepositoryPath(name);         
            DirectoryHelper.DeleteRecursively(directoryPath);      
        }
      





    }
}