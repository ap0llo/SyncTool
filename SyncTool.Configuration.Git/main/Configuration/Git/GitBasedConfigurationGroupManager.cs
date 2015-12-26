// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using SyncTool.Common.Utilities;
using SyncTool.Configuration.Model;
using SyncTool.FileSystem;
using SyncTool.FileSystem.Git;
using NativeDirectory = System.IO.Directory;

namespace SyncTool.Configuration.Git
{
    public sealed class GitBasedConfigurationGroupManager : GitBasedGroupManager<IConfigurationGroup>
    {
        
        public GitBasedConfigurationGroupManager(IRepositoryPathProvider pathProvider) : base(pathProvider)
        {            
        }



        public override IConfigurationGroup GetGroup(string name) => new GitBasedConfigurationGroup(GetRepositoryPath(name));

    }
}