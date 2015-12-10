// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using SyncTool.Common;
using SyncTool.Common.Utilities;
using NativeDirectory = System.IO.Directory;

namespace SyncTool.FileSystem.Git
{
    public abstract class GitBasedGroupManager<T> : IGroupManager<T> where T : IGroup
    {
        protected readonly IRepositoryPathProvider m_PathProvider;


        public IEnumerable<string> Groups
        {
            get
            {
                foreach (var dir in m_PathProvider.RepositoryPaths)
                {
                    using (var group = new GitBasedGroup(dir))
                    {
                        yield return group.Name;
                    }
                }
            }
        }

        
       
        protected GitBasedGroupManager(IRepositoryPathProvider pathProvider)
        {
            if (pathProvider == null)
            {
                throw new ArgumentNullException(nameof(pathProvider));
            }
            m_PathProvider = pathProvider;
        }



        public abstract T GetGroup(string name);

        public void AddGroup(string name)
        {
            if (Groups.Contains(name, StringComparer.CurrentCultureIgnoreCase))
            {
                throw new DuplicateGroupException(name);
            }

            var directoryPath = m_PathProvider.GetRepositoryPath(name);

            if (NativeDirectory.Exists(directoryPath))
            {
                throw new GroupManagerException($"Cannot create repository for SyncGroup '{name}'. Directory already exists");
            }

            NativeDirectory.CreateDirectory(directoryPath);
            RepositoryInitHelper.InitializeRepository(directoryPath, name);
        }


        public void RemoveGroup(string name)
        {
            if (Groups.Contains(name, StringComparer.InvariantCultureIgnoreCase) == false)
            {
                throw new GroupNotFoundException(name);
            }

            var directoryPath = m_PathProvider.GetRepositoryPath(name);
            DirectoryHelper.DeleteRecursively(directoryPath);
        }

        protected string GetRepositoryPath(string name)
        {
            var directories = m_PathProvider.RepositoryPaths;
            foreach (var dir in directories)
            {
                using (var group = new GitBasedGroup(dir))
                {
                    if (group.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return dir;
                    }                    
                }
                    
            }
            
            throw new GroupNotFoundException(name);
        }



    }
}