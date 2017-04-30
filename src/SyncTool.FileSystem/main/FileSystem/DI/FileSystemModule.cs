using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncTool.FileSystem.FileSystem.DI
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
