// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using Ninject.Modules;
using SyncTool.FileSystem.Versioning;

namespace SyncTool.FileSystem.Git
{
    public class GitFileSystemModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IHistoryRepository>().To<GitBasedHistoryRepository>();
        }
    }
}