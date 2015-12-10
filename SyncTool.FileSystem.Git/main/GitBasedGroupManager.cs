// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using SyncTool.Common;

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

        
       
        public GitBasedGroupManager(IRepositoryPathProvider pathProvider)
        {
            if (pathProvider == null)
            {
                throw new ArgumentNullException(nameof(pathProvider));
            }
            m_PathProvider = pathProvider;
        }



        public abstract T GetGroup(string name);


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