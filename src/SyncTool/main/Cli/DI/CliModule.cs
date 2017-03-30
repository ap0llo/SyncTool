// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Ninject;
using Ninject.Modules;
using SyncTool.Cli.Framework;
using SyncTool.Cli.Output;
using SyncTool.FileSystem;

namespace SyncTool.Cli.DI
{
    public class CliModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ICommandFactory>().To<NinjectCommandFactory>().WithConstructorArgument(typeof(IKernel), this.Kernel);
            Bind<ICommandLoader>().To<CurrentAssemblyCommandLoader>();
            Bind<IOutputWriter>().To<ConsoleOutputWriter>();


            Bind<IEqualityComparer<IFile>>().To<FilePropertiesComparer>();

        }
    }
}