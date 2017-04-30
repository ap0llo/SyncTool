using System.Collections.Generic;
using Autofac;
using SyncTool.Cli.Framework;
using SyncTool.Cli.Output;
using SyncTool.FileSystem;
using Assembly = System.Reflection.Assembly;

namespace SyncTool.Cli.DI
{
    public class CliModule : Module
    {

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AutoFacCommandFactory>().As<ICommandFactory>();
            builder.RegisterType<AutofacCommandLoader>().As<ICommandLoader>();
            builder.RegisterType<ConsoleOutputWriter>().As<IOutputWriter>();
            builder.RegisterType<Application>().AsSelf();

            builder
                .RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .AssignableTo<ICommand>()
                .AsSelf()
                .As<ICommand>();

            //TODO: Should this be in the FileSystem module?
            builder.RegisterType<FilePropertiesComparer>().As<IEqualityComparer<IFile>>();

            base.Load(builder);
        }
        
    }
}