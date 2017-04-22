using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SyncTool.Cli.Framework
{
    public abstract class AbstractCommandLoader : ICommandLoader
    {
        /// <summary>
        /// Gets all the available commands
        /// </summary>
        /// <returns></returns>
        public CommandDescription[] GetCommands()
        {
            return GetCommandAssemblies().SelectMany(GetCommands).ToArray();
        }

        /// <summary>
        /// Gets all the command types from the specified assembly
        /// </summary>
        protected IEnumerable<CommandDescription> GetCommands(Assembly assembly)
        {
            return assembly.GetTypes()
                .Where(IsCommandType)
                .Select(GetCommandDescription);
        }

        /// <summary>
        /// Gets the list of assemblies to search for command implementations
        /// </summary>
        /// <returns></returns>
        protected abstract IEnumerable<Assembly> GetCommandAssemblies();

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