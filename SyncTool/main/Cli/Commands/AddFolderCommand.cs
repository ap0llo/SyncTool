// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using System.IO;
using CommandLine;
using SyncTool.Cli.Framework;
using SyncTool.Cli.Output;
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



    public class AddFolderCommand : CommandBase, ICommand<AddFolderOptions>
    {
        readonly IGroupManager m_GroupManager;
        

        public AddFolderCommand(IOutputWriter outputWriter, IGroupManager groupManager) : base(outputWriter)
        {
            if (groupManager == null)
            {
                throw new ArgumentNullException(nameof(groupManager));
            }
            m_GroupManager = groupManager;            
        }


        public int Run(AddFolderOptions opts)
        {
            if (opts.Filter != null && opts.FilterFilePath != null)
            {
                throw new InvalidOperationException("Unexpected code path. Should have been handled by commandline parser.");
            }

            using (var group = m_GroupManager.GetGroup(opts.Group))            
            {
                var filter = FilterConfiguration.Empty;
                                
                if (opts.Filter != null)
                {
                    filter = new FilterConfiguration(FilterType.MicroscopeQuery, opts.Filter);
                }
                else if (opts.FilterFilePath != null)
                {
                    if (!File.Exists(opts.FilterFilePath))
                    {
                        OutputWriter.WriteErrorLine($"File '{opts.FilterFilePath}' not found");
                        return 1;
                    }

                    filter = new FilterConfiguration(FilterType.MicroscopeQuery, File.ReadAllText(opts.FilterFilePath));                    
                }

                var configService = group.GetService<IConfigurationService>();
                configService.AddItem(new SyncFolder(opts.Name) { Path = opts.Path, Filter = filter });

                var historyService = group.GetService<IHistoryService>();
                historyService.CreateHistory(opts.Name);
            }
            return 0;
        }
    }
}