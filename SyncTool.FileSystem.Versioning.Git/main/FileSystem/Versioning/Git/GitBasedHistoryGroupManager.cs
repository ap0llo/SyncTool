// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using SyncTool.FileSystem.Git;

namespace SyncTool.FileSystem.Versioning.Git
{
    public class GitBasedHistoryGroupManager : GitBasedGroupManager<IHistoryGroup>
    {
        

        public GitBasedHistoryGroupManager(IRepositoryPathProvider pathProvider) : base(pathProvider)
        {
           
        }
 

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
 