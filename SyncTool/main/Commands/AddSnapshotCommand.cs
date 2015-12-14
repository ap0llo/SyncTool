// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using CommandLine;
using SyncTool.Cli.Framework;
using SyncTool.Common;
using SyncTool.Configuration.Model;
using SyncTool.FileSystem.Local;
using SyncTool.FileSystem.Versioning;

namespace SyncTool.Cli.Commands
{
    [Verb("Add-Snapshot")]
    public class AddSnapshotOptions
    {

        [Option(Required = true)]
        public string Group { get; set; }

        [Option(Required = true)]
        public string Folder { get; set; }

    }

    public class AddSnapshotCommand : ICommand<AddSnapshotOptions>
    {
        readonly IGroupManager<IConfigurationGroup> m_ConfigurationGroupManager;
        readonly IGroupManager<IHistoryGroup> m_HistoryGroupManager;

        public AddSnapshotCommand(IGroupManager<IConfigurationGroup> configurationGroupManager, IGroupManager<IHistoryGroup> historyGroupManager)
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



        public int Run(AddSnapshotOptions opts)
        {
            using (var group = m_ConfigurationGroupManager.GetGroup(opts.Group))
            using (var historyRepository = m_HistoryGroupManager.GetGroup(opts.Group))
            {
                var folder = group.GetItem(opts.Folder);
                var history = historyRepository.GetItem(opts.Folder);

                var state = new LocalDirectory(null, folder.Path);

                //TODO: Apply filter

                history.CreateSnapshot(state);

                return 0;

            }

        }
    }
}