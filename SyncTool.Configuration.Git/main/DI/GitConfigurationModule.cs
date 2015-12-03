// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using Ninject.Modules;
using SyncTool.Configuration.Model;

namespace SyncTool.Configuration.Git.DI
{
    public class GitConfigurationModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<ISyncGroupManager>().To<GitBasedSyncGroupManager>().InSingletonScope();
        }
    }
}