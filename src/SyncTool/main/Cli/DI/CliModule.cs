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