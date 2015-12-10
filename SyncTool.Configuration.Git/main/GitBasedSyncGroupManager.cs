// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using Ninject;
using SyncTool.Configuration.Model;
using SyncTool.FileSystem.Git;
using SyncTool.Utilities;
using NativeDirectory = System.IO.Directory;

namespace SyncTool.Configuration.Git
{
    public sealed class GitBasedSyncGroupManager : ISyncGroupManager
    {
        readonly IRepositoryPathProvider m_PathProvider;

        
        public IEnumerable<string> SyncGroups
        {
            get
            {
                foreach (var dir in m_PathProvider.RepositoryPaths)
                {                    
                    using (var group = new GitBasedSyncGroup(dir))
                    {
                        yield return group.Name; 
                    }
                }                
            }
        }

        public ISyncGroup GetSyncGroup(string name)
        {
            var directories = m_PathProvider.RepositoryPaths;
            foreach (var dir in directories)
            {
                var group = new GitBasedSyncGroup(dir);
                if (group.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    return group;
                }
                else
                {
                    group.Dispose();
                }
            }

            throw new SyncGroupNotFoundException(name);            
        }

        
        public GitBasedSyncGroupManager(IRepositoryPathProvider pathProvider)
        {
            if (pathProvider == null)
            {
                throw new ArgumentNullException(nameof(pathProvider));
            }
            m_PathProvider = pathProvider;                        
        }


        public ISyncGroup AddSyncGroup(string name)
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

            return new GitBasedSyncGroup(directoryPath);
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
      

        public void Dispose()
        {            
        }






    }
}