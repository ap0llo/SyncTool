using System;
using Autofac;
using SyncTool.Cli.Framework;

namespace SyncTool.Cli.DI
{
    /// <summary>
    /// Implementation of <see cref="ICommandFactory"/> that uses Ninject to create command instances
    /// </summary>
    public class AutoFacCommandFactory : ICommandFactory
    {
        readonly IComponentContext m_Container;

        public AutoFacCommandFactory(IComponentContext container)
        {
            m_Container = container ?? throw new ArgumentNullException(nameof(container));
        }

        public object CreateCommandInstance(Type commandType)
        {
            if (commandType == null)
            {
                throw new ArgumentNullException(nameof(commandType));
            }

            return m_Container.Resolve(commandType);
        }
    }
}