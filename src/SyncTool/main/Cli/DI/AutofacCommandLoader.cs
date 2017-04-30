using Autofac;
using Autofac.Core;
using SyncTool.Cli.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncTool.Cli.DI
{
    class AutofacCommandLoader : ICommandLoader
    {
        readonly IComponentContext m_Container;

        public AutofacCommandLoader(IComponentContext container)
        {
            m_Container = container ?? throw new ArgumentNullException(nameof(container));
        }


        public CommandDescription[] GetCommands()
        {
            return m_Container
                .GetImplementingTypes<ICommand>()
                .Select(GetCommandDescription)
                .ToArray();            
        }

        /// <summary>
        /// Gets the CommandDescription for the specified command type
        /// </summary>
        CommandDescription GetCommandDescription(Type commandType)
        {
            return new CommandDescription()
            {
                ImplementationType = commandType,
                OptionType = GetOptionType(commandType)
            };
        }

        bool IsCommandType(Type type)
        {
            return type.GetInterfaces().Any(IsCommandInterfaceType);
        }

        bool IsCommandInterfaceType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ICommand<>);
        }

        Type GetOptionType(Type commandType)
        {
            return commandType.GetInterfaces()
                              .First(IsCommandInterfaceType)
                              .GetGenericArguments().Single();
        }

    }
}
