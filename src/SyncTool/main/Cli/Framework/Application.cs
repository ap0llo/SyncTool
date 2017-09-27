using System;
using System.Linq;
using System.Reflection;
using CommandLine;
using System.Diagnostics;

namespace SyncTool.Cli.Framework
{
    public class Application
    {
        readonly ICommandFactory m_CommandFactory;
        readonly ICommandLoader m_CommandLoader;


        public Application(ICommandFactory commandFactory, ICommandLoader commandLoader)
        {
            m_CommandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            m_CommandLoader = commandLoader ?? throw new ArgumentNullException(nameof(commandLoader));
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
            OptionsBase optionInstance = null;

            parser.ParseArguments(args, commands.Select(c => c.OptionType).ToArray())
                .WithParsed(obj =>
                {
                    optionInstance = (OptionsBase) obj;
                    selectedCommand = commands.Single(c => c.OptionType == obj.GetType());
                });

           
            if (optionInstance?.LaunchDebugger == true)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
                else
                {
                    Debugger.Launch();
                }            
            }


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


        int ExecuteCommand(CommandDescription command, OptionsBase optionInstance)
        {
            // get MethodInfo for the "Run" method
            var runMethod = GetRunMethod(command);
            
            // create command and execute it
            var commandInstance = m_CommandFactory.CreateCommandInstance(command.ImplementationType);

            try
            {
                var result = (int)runMethod.Invoke(commandInstance, new[] { optionInstance });

                // call dispose if command implements IDisposable
                (commandInstance as IDisposable)?.Dispose();

                return result;
            }
            catch (TargetInvocationException e) when (e.InnerException != null)
            {
                throw e.InnerException;
            }
        }

        MethodInfo GetRunMethod(CommandDescription command)
        {
            var interfaceMapping = command.ImplementationType.GetInterfaceMap(typeof(ICommand<>).MakeGenericType(command.OptionType));
            
            for (int i = 0; i < interfaceMapping.InterfaceMethods.Length; i++)
            {
                if (interfaceMapping.InterfaceMethods[i].Name == nameof(ICommand<OptionsBase>.Run))
                {
                    return interfaceMapping.TargetMethods[i];                    
                }
            }

            throw new ArgumentException($"Could not find Run() method for CommandDescription [{command}]. Make sure the implementation class implements {typeof(ICommand<>).Name}");
        }
    }
}
