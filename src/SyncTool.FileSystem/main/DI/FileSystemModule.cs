using System.Collections.Generic;
using Autofac;

namespace SyncTool.FileSystem.DI
{
    public class FileSystemModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<FilePropertiesComparer>().As<IEqualityComparer<IFile>>();
            builder.RegisterInstance(EqualityComparer<IFileReference>.Default).As<IEqualityComparer<IFileReference>>();

            base.Load(builder);
        }
    }
}
