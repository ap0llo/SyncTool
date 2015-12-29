// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using SyncTool.Configuration.Model;
using SyncTool.Git.Common;
using SyncTool.Git.FileSystem;

namespace SyncTool.Git.Configuration
{
    public sealed class GitBasedConfigurationGroupManager : GitBasedGroupManager<IConfigurationGroup>
    {
        
        public GitBasedConfigurationGroupManager(IRepositoryPathProvider pathProvider) : base(pathProvider)
        {            
        }



        public override IConfigurationGroup GetGroup(string name) => new GitBasedConfigurationGroup(GetRepositoryPath(name));

    }
}