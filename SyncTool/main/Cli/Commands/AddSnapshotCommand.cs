// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using CommandLine;
using SyncTool.Cli.Framework;
using SyncTool.Cli.Output;
using SyncTool.Common;
using SyncTool.Configuration.Model;
using SyncTool.FileSystem.Local;
using SyncTool.FileSystem.Versioning;

namespace SyncTool.Cli.Commands
{
    [Verb("Add-Snapshot")]
    public class AddSnapshotOptions
    {

        [Option('g', "group", Required = true)]
        public string Group { get; set; }

        [Option('f', "folder", Required = true)]
        public string Folder { get; set; }

    }

    public class AddSnapshotCommand : CommandBase, ICommand<AddSnapshotOptions>
    {
        readonly IGroupManager m_GroupManager;
        

        public AddSnapshotCommand(IOutputWriter outputWriter, IGroupManager groupManager) : base(outputWriter)
        {
            if (groupManager == null)
            {
                throw new ArgumentNullException(nameof(groupManager));
            }
            m_GroupManager = groupManager;        
        }



        public int Run(AddSnapshotOptions opts)
        {
            using (var group = m_GroupManager.GetGroup(opts.Group))
            {
                var configurationService = group.GetService<IConfigurationService>();
                var historyService = group.GetService<IHistoryService>();

                var folder = configurationService[opts.Folder];
                var history = historyService[opts.Folder];

                var state = new LocalDirectory(null, folder.Path);

                //TODO: Apply filter

                history.CreateSnapshot(state);

                return 0;            
            }
        }
    }
}