// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using Ninject;
using Ninject.Modules;
using SyncTool.Cli.Framework;

namespace SyncTool.Cli.DI
{
    public class CliModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ICommandFactory>().To<NinjectCommandFactory>().WithConstructorArgument(typeof(IKernel), this.Kernel);
            Bind<ICommandLoader>().To<CurrentAssemblyCommandLoader>();
        }
    }
}