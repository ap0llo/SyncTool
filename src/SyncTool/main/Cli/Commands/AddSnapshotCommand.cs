using System;
using CommandLine;
using SyncTool.Cli.Framework;
using SyncTool.Cli.Output;
using SyncTool.Common.Groups;
using SyncTool.Configuration.Model;
using SyncTool.FileSystem.Local;
using SyncTool.FileSystem.Versioning;

namespace SyncTool.Cli.Commands
{
    [Verb("Add-Snapshot")]
    public class AddSnapshotOptions : OptionsBase
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
            m_GroupManager = groupManager ?? throw new ArgumentNullException(nameof(groupManager));        
        }



        public int Run(AddSnapshotOptions opts)
        {
            using (var group = m_GroupManager.OpenExclusively(opts.Group))
            {
                var configurationService = group.GetService<IConfigurationService>();
                var historyService = group.GetService<IHistoryService>();

                var folder = configurationService[opts.Folder];
                var history = historyService[opts.Folder];

                var state = new LocalDirectory(null, folder.Path);                

                history.CreateSnapshot(state);

                return 0;            
            }
        }
    }
}