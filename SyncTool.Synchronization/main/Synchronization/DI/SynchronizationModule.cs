// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using Ninject.Modules;

namespace SyncTool.Synchronization.DI
{
    public class SynchronizationModule : NinjectModule
    {
        public override void Load()
        {
            //this.Bind<ISynchronizer>().To<Synchronizer>();
        }
    }
}