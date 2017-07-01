using Autofac;
using System;
using SyncTool.Common.Groups;

namespace SyncTool.Common.DI
{
    /// <summary>
    /// Module for components registered in the Group scope, independent of the Group type
    /// </summary>
    class CommonGroupScopeModule : Module
    {
        readonly GroupStorage m_GroupStorage;
        readonly GroupSettings m_GroupSettings;


        public CommonGroupScopeModule(GroupStorage groupStorage, GroupSettings groupSettings)
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
