using System;
using SyncTool.Common;
using Autofac;
using Autofac.Core.Registration;

namespace SyncTool.Common
{
    public class Group : IGroup
    {
        readonly GroupSettings m_Settings;
        readonly ILifetimeScope m_GroupScope;


        public event EventHandler Disposed;


        public string Name => m_Settings.Name;


        public Group(GroupSettings groupSettings, ILifetimeScope groupScope)
        {
            m_Settings = groupSettings ?? throw new ArgumentNullException(nameof(groupSettings));
            m_GroupScope = groupScope ?? throw new ArgumentNullException(nameof(groupScope));
        }


        public T GetService<T>() 
        {
            try
            {
                return m_GroupScope.Resolve<T>();
            }
            catch (ComponentNotRegisteredException ex)
            {
                throw new ServiceNotFoundException(typeof(T), ex);
            }
        }

        public void Dispose() => Disposed?.Invoke(this, EventArgs.Empty);

    }
}