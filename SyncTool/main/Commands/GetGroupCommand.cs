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
        readonly IGroupManager<IConfigurationGroup> m_ConfigurationGroupManager;        


        public GetGroupCommand(IOutputWriter outputWriter, IGroupManager<IConfigurationGroup> configurationGroupManager) : base(outputWriter)
        {
            if (configurationGroupManager == null)
            {
                throw new ArgumentNullException(nameof(configurationGroupManager));
            }

            m_ConfigurationGroupManager = configurationGroupManager;
        }


        public int Run(GetGroupOptions opts)
        {
            var groupNames = String.IsNullOrEmpty(opts.Name)
                    ? m_ConfigurationGroupManager.Groups
                    : m_ConfigurationGroupManager.Groups.Where(g => g.Equals(opts.Name, StringComparison.InvariantCultureIgnoreCase));

            foreach (var group in groupNames.Select(m_ConfigurationGroupManager.GetGroup))
            {
                using (group)
                {
                    PrintSyncGroup(group);
                }
            }

            return 0;
        }


        void PrintSyncGroup(IConfigurationGroup group)
        {                       
            OutputWriter.WriteLine($"SyncGroup '{group.Name}'");
            OutputWriter.WriteLine();

            if (group.Items.Any())
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
                        group.Items.Select(x => x.Name).ToArray(),
                        group.Items.Select(x => x.Path).ToArray(),
                        group.Items.Select(x => x.Filter?.Query).Select(x => x ?? "").ToArray(),
                    });
            }
            else
            {                
                OutputWriter.WriteLine(" \tNo folders in this sync group");                
            }
        }
        
    }


}