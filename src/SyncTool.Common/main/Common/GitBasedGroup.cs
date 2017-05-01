using System;
using SyncTool.Common;
using Autofac;
using Autofac.Core.Registration;

namespace SyncTool.Common
{
    public class Group : IGroup
    {                
        readonly ILifetimeScope m_GroupScope;


        public event EventHandler Disposed;


        public string Name { get; }        


        public Group(GroupSettings groupSettings, ILifetimeScope groupScope)
        {
            m_GroupScope = groupScope ?? throw new ArgumentNullException(nameof(groupScope));
            
            //TODO: Remove checks once they have been implemented in GroupSettings
            var name = groupSettings.Name;
            
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (String.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name must not be empty", nameof(name));
            }
            
            Name = name;            
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

        public void Dispose()
        {         
            Disposed?.Invoke(this, EventArgs.Empty);
        }        
    }
}