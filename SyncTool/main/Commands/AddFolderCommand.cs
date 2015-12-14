// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
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
        [Option(Required = true)]
        public string Group { get; set; }

        [Option(Required = true)]
        public string Name { get; set; }

        [Option(Required = true)]
        public string Path { get; set; }
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
            using (var syncGroup = m_ConfigurationGroupManager.GetGroup(opts.Group))
            using (var historyRepository = m_HistoryGroupManager.GetGroup(opts.Group))
            {
                syncGroup.AddSyncFolder(new SyncFolder() { Name = opts.Name, Path = opts.Path });
                historyRepository.CreateHistory(opts.Name);
            }
            return 0;
        }
    }
}