// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using Ninject.Modules;

namespace SyncTool.FileSystem.Git.DI
{
    public class GitFileSystemModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IRepositoryPathProvider>().To<CurrentDirectoryRepositoryPathProvider>();
        }
    }
}