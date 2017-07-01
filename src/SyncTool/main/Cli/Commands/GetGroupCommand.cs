using System;
using System.Linq;
using CommandLine;
using SyncTool.Cli.Framework;
using SyncTool.Cli.Output;
using SyncTool.Common.Groups;
using SyncTool.Configuration;

namespace SyncTool.Cli.Commands
{
    [Verb("Get-Group")]
    public class GetGroupOptions : OptionsBase
    {
        [Option('n', "name", Required = false)]
        public string Name { get; set; }
    }


    public class GetGroupCommand : CommandBase, ICommand<GetGroupOptions>
    {
        readonly IGroupManager m_GroupManager;        


        public GetGroupCommand(IOutputWriter outputWriter, IGroupManager groupManager) : base(outputWriter)
        {
            m_GroupManager = groupManager ?? throw new ArgumentNullException(nameof(groupManager));
        }


        public int Run(GetGroupOptions opts)
        {
            var groupNames = String.IsNullOrEmpty(opts.Name)
                    ? m_GroupManager.Groups
                    : m_GroupManager.Groups.Where(g => g.Equals(opts.Name, StringComparison.InvariantCultureIgnoreCase));

            foreach (var group in groupNames.Select(m_GroupManager.OpenExclusively))
            {
                using (group)
                {
                    OutputWriter.WriteLine($"SyncGroup '{group.Name}'");
                    OutputWriter.WriteLine();

                    var configurationService = group.GetService<IConfigurationService>();
                    Print(configurationService);
                }
            }

            return 0;
        }


        void Print(IConfigurationService service)
        {   
            if (service.Items.Any())
            {
                OutputWriter.WriteTable(
                    new []
                    {
                        "Folder",
                        "Path"
                    },
                    new []
                    {
                        service.Items.Select(x => x.Name).ToArray(),
                        service.Items.Select(x => x.Path).ToArray()                        
                    });
            }
            else
            {                
                OutputWriter.WriteLine(" \tNo folders in this sync group");                
            }
        }
        
    }


}