using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncTool.Common.DI
{
    class CommonGroupModule : Module
    {
        readonly GroupStorage m_GroupStorage;
        readonly GroupSettings m_GroupSettings;


        public CommonGroupModule(GroupStorage groupStorage, GroupSettings groupSettings)
        {
            m_GroupStorage = groupStorage ?? throw new ArgumentNullException(nameof(groupStorage));
            m_GroupSettings = groupSettings;
        }

        protected override void Load(ContainerBuilder builder)
        {            
            if (m_GroupSettings != null)
            {
                builder.RegisterInstance(m_GroupSettings).AsSelf().ExternallyOwned();
            }

            builder.RegisterInstance(m_GroupStorage).AsSelf();
        }

    }
}
