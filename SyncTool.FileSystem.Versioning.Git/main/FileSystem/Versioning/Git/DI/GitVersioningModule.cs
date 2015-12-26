// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using Ninject.Modules;
using SyncTool.Common;

namespace SyncTool.FileSystem.Versioning.Git.DI
{
    public class GitVersioningModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IGroupManager<IHistoryGroup>>().To<GitBasedHistoryGroupManager>();
        }
    }
}