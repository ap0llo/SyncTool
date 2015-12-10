// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using SyncTool.FileSystem.Git;

namespace SyncTool.FileSystem.Versioning.Git
{
    public class GitBasedHistoryRepositoryManager : GitBasedGroupManager<IHistoryGroup>
    {
        [Obsolete]
        public IEnumerable<string> HistoryRepositories => Groups;
        

        public GitBasedHistoryRepositoryManager(IRepositoryPathProvider pathProvider) : base(pathProvider)
        {
           
        }


        [Obsolete]
        public IHistoryGroup GetHistoryRepository(string name) => GetGroup(name);
 

        public override IHistoryGroup GetGroup(string name)
        {
            try
            {
                return new GitBasedHistoryGroup(GetRepositoryPath(name));
            }
            catch (GroupNotFoundException ex)
            {
                throw new HistoryGroupNotFoundException($"HistoryRepository '{name}' not found", ex);
            }
        }
      


    }
}
 