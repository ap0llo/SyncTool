using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using SyncTool.Git.Common;
using SyncTool.Common;

namespace SyncTool.Git.DI
{
    public class GitModuleFactoryModule : Module
    {

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<GitGroupModuleFactory>().As<IGroupModuleFactory>();
        }

    }
}
