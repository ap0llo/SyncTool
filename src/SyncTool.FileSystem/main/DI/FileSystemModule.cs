using System.Collections.Generic;
using Autofac;
using NodaTime;

namespace SyncTool.FileSystem.DI
{
    public class FileSystemModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(SystemClock.Instance).As<IClock>();

            builder.RegisterType<FilePropertiesComparer>().As<IEqualityComparer<IFile>>();
            builder.RegisterInstance(EqualityComparer<FileReference>.Default).As<IEqualityComparer<FileReference>>();

            base.Load(builder);
        }
    }
}
