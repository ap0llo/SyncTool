// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using Ninject.Modules;

namespace SyncTool.FileSystem.Versioning.Git
{
    public class GitFileSystemVersioningModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IHistoryRepository>().To<GitBasedHistoryRepository>();
        }
    }
}