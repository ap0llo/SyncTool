// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Reflection;
using CommandLine;
using Ninject;

namespace SyncTool.Cli.Framework
{
    public class Application
    {
        readonly ICommandFactory m_CommandFactory;
        readonly ICommandLoader m_CommandLoader;


        public Application(ICommandFactory commandFactory, ICommandLoader commandLoader)
        {
            if (commandFactory == null)
            {
                throw new ArgumentNullException(nameof(commandFactory));
            }
            if (commandLoader == null)
            {
                throw new ArgumentNullException(nameof(commandLoader));
            }
            m_CommandFactory = commandFactory;
            m_CommandLoader = commandLoader;
        }


        public int Run(string[] args)
        {
            // get available commands
            var commands = m_CommandLoader.GetCommands();
            
            // parse args
            var parser = new Parser(opts =>
            {
                opts.CaseSensitive = false;
                opts.HelpWriter = Parser.Default.Settings.HelpWriter;
            });

            
            // select command
            CommandDescription? selectedCommand = null;
            object optionInstance = null;

            parser.ParseArguments(args, commands.Select(c => c.OptionType).ToArray())
                .WithParsed(obj =>
                {
                    optionInstance = obj;
                    selectedCommand = commands.Single(c => c.OptionType == obj.GetType());
                });


            // execute command
            if (selectedCommand.HasValue)
            {
                return ExecuteCommand(selectedCommand.Value, optionInstance);
            }
            else
            {
                return 1;                
            }
            
        }


        int ExecuteCommand(CommandDescription command, object optionInstance)
        {
            // get MethodInfo for the "Run" method
            var runMethod = GetRunMethod(command);
            
            // create command and execute it
            var commandInstance = m_CommandFactory.CreateCommandInstance(command.ImplementationType);

            var result = (int)runMethod.Invoke(commandInstance, new[] { optionInstance });

            // call dispose if command implements IDisposable
            (commandInstance as IDisposable)?.Dispose();

            return result;
        }

        MethodInfo GetRunMethod(CommandDescription command)
        {
            var interfaceMapping = command.ImplementationType.GetInterfaceMap(typeof(ICommand<>).MakeGenericType(command.OptionType));
            
            for (int i = 0; i < interfaceMapping.InterfaceMethods.Length; i++)
            {
                if (interfaceMapping.InterfaceMethods[i].Name == nameof(ICommand<object>.Run))
                {
                    return interfaceMapping.TargetMethods[i];                    
                }
            }

            throw new ArgumentException($"Could not find Run() method for CommandDescription [{command}]. Make sure the implementation class implements {typeof(ICommand<>).Name}");
        }




    }
}