// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using CommandLine;
using SyncTool.Cli;
using SyncTool.Cli.Framework;
using SyncTool.Common;
using SyncTool.Configuration.Model;
using SyncTool.FileSystem.Versioning;

namespace SyncTool.Cli.Commands
{
    [Verb("Get-Group")]
    public class GetGroupOptions
    {
        [Option(Required = false)]
        public string Name { get; set; }
    }


    public class GetGroupCommand : ICommand<GetGroupOptions>
    {
        readonly IGroupManager<IConfigurationGroup> m_ConfigurationGroupManager;        


        public GetGroupCommand(IGroupManager<IConfigurationGroup> configurationGroupManager)
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

            Console.WriteLine();
            foreach (var group in groupNames.Select(m_ConfigurationGroupManager.GetGroup))
            {
                using (group)
                {
                    PrintSyncGroup(group, " ");
                    Console.WriteLine();
                }
            }

            return 0;
        }


        void PrintSyncGroup(IConfigurationGroup group, string prefix = "")
        {
            Console.WriteLine($"{prefix}SyncGroup '{group.Name}'");

            if (group.Items.Any())
            {
                Console.WriteLine($"{prefix}\tFolders:");
                foreach (var folder in group.Items)
                {
                    PrintSyncFolder(folder, $"{prefix}\t\t");
                }
            }
            else
            {
                Console.WriteLine(" \tNo folders in this sync group");
            }
        }

        void PrintSyncFolder(SyncFolder folder, string prefix = "")
        {
            Console.WriteLine($"{prefix}{folder.Name} --> {folder.Path}");
        }
    }


}