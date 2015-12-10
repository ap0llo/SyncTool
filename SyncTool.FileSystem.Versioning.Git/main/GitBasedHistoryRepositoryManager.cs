// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using SyncTool.FileSystem.Git;

namespace SyncTool.FileSystem.Versioning.Git
{
    public class GitBasedHistoryRepositoryManager : GitBasedGroupManager, IHistoryRepositoryManager
    {

        public IEnumerable<string> HistoryRepositories => Groups;
        

        public GitBasedHistoryRepositoryManager(IRepositoryPathProvider pathProvider) : base(pathProvider)
        {
           
        }



        public IHistoryRepository GetHistoryRepository(string name)
        {
            try
            {
                return new GitBasedHistoryRepository(GetRepositoryPath(name));
            }
            catch (GroupNotFoundException ex)
            {
                throw new HistoryRepositoryNotFoundException($"HistoryRepository '{name}' not found", ex);
            }
        }

      


    }
}
 