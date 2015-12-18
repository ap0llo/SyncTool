// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using System.IO;
using CommandLine;
using SyncTool.Cli.Framework;
using SyncTool.Common;
using SyncTool.Configuration.Model;
using SyncTool.FileSystem.Versioning;

namespace SyncTool.Cli.Commands
{
    [Verb("Add-Folder"),]
    public class AddFolderOptions
    {
        [Option('g', "group", Required = true)]
        public string Group { get; set; }

        [Option('n', "name", Required = true)]
        public string Name { get; set; }

        [Option('p', "path", Required = true)]
        public string Path { get; set; }        

        [Option("filter", Required = false, SetName = "FilterFromArg")]
        public string Filter { get; set; }

        [Option("importFilter", Required = false, SetName = "FilterFromFile")]
        public string FilterFilePath { get; set; }
    }



    public class AddFolderCommand : ICommand<AddFolderOptions>
    {
        readonly IGroupManager<IConfigurationGroup> m_ConfigurationGroupManager;
        readonly IGroupManager<IHistoryGroup> m_HistoryGroupManager;


        public AddFolderCommand(IGroupManager<IConfigurationGroup> configurationGroupManager, IGroupManager<IHistoryGroup> historyGroupManager)
        {
            if (configurationGroupManager == null)
            {
                throw new ArgumentNullException(nameof(configurationGroupManager));
            }
            if (historyGroupManager == null)
            {
                throw new ArgumentNullException(nameof(historyGroupManager));
            }
            m_ConfigurationGroupManager = configurationGroupManager;
            m_HistoryGroupManager = historyGroupManager;
        }


        public int Run(AddFolderOptions opts)
        {
            if (opts.Filter != null && opts.FilterFilePath != null)
            {
                throw new InvalidOperationException("Unexpected code path. Should have been handled by commandline parser.");
            }


            using (var syncGroup = m_ConfigurationGroupManager.GetGroup(opts.Group))
            using (var historyRepository = m_HistoryGroupManager.GetGroup(opts.Group))
            {
                var filter = new FileSystemFilterConfiguration() { Type = FileSystemFilterType.MicroscopeQuery };
                if (opts.Filter != null)
                {
                    filter.Query = opts.Filter;
                }
                else if (opts.FilterFilePath != null)
                {
                    if (!File.Exists(opts.FilterFilePath))
                    {
                        Console.WriteLine($"Error: File '{opts.FilterFilePath}' not found");
                        return 1;
                    }

                    filter.Query = File.ReadAllText(opts.FilterFilePath);
                }

                syncGroup.AddSyncFolder(new SyncFolder() { Name = opts.Name, Path = opts.Path, Filter = filter });
                historyRepository.CreateHistory(opts.Name);
            }
            return 0;
        }
    }
}