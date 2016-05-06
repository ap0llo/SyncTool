// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using CommandLine;
using SyncTool.Cli.Framework;
using SyncTool.Cli.Output;
using SyncTool.Common;
using SyncTool.Configuration.Model;

namespace SyncTool.Cli.Commands
{
    [Verb("Get-Group")]
    public class GetGroupOptions
    {
        [Option('n', "name", Required = false)]
        public string Name { get; set; }
    }


    public class GetGroupCommand : CommandBase, ICommand<GetGroupOptions>
    {
        readonly IGroupManager m_GroupManager;        


        public GetGroupCommand(IOutputWriter outputWriter, IGroupManager groupManager) : base(outputWriter)
        {
            if (groupManager == null)
            {
                throw new ArgumentNullException(nameof(groupManager));
            }

            m_GroupManager = groupManager;
        }


        public int Run(GetGroupOptions opts)
        {
            var groupNames = String.IsNullOrEmpty(opts.Name)
                    ? m_GroupManager.Groups
                    : m_GroupManager.Groups.Where(g => g.Equals(opts.Name, StringComparison.InvariantCultureIgnoreCase));

            foreach (var group in groupNames.Select(m_GroupManager.GetGroup))
            {
                using (group)
                {
                    var configurationService = group.GetService<IConfigurationService>();
                    Print(configurationService);
                }
            }

            return 0;
        }


        void Print(IConfigurationService service)
        {                       
            OutputWriter.WriteLine($"SyncGroup '{service.Group.Name}'");
            OutputWriter.WriteLine();

            if (service.Items.Any())
            {
                OutputWriter.WriteTable(
                    new []
                    {
                        "Folder",
                        "Path",
                        "Filter"
                    },
                    new []
                    {
                        service.Items.Select(x => x.Name).ToArray(),
                        service.Items.Select(x => x.Path).ToArray(),
                        service.Items.Select(x => x.Filter?.CustomData).Select(x => x ?? "").ToArray(),
                    });
            }
            else
            {                
                OutputWriter.WriteLine(" \tNo folders in this sync group");                
            }
        }
        
    }


}