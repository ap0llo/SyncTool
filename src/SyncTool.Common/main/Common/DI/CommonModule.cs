using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncTool.Common.DI
{
    public class CommonModule : Module
    {

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Group>()
                .AsSelf()
                .As<IGroup>()
                .InstancePerMatchingLifetimeScope(Scope.Group)
                .ExternallyOwned();

            builder.RegisterType<CurrentDirectoryGroupDirectoryPathProvider>().As<IGroupDirectoryPathProvider>();

            base.Load(builder);
        }

    }
}
