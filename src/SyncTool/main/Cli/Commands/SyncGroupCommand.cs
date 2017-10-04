using System;
using CommandLine;
using SyncTool.Cli.Framework;
using SyncTool.Cli.Output;
using SyncTool.Common.Groups;
using SyncTool.Synchronization;

namespace SyncTool.Cli.Commands
{    

    [Verb("Sync-Group")]
    public class SyncGroupOptions : OptionsBase
    {
        [Option('g', "group", Required = true)]
        public string Group { get; set; }
     
    }

    public class SyncGroupCommand : CommandBase, ICommand<SyncGroupOptions>
    {        
        readonly IGroupManager m_GroupManager;
        
        public SyncGroupCommand(IOutputWriter outputWriter, IGroupManager groupManager) : base(outputWriter)
        {
            m_GroupManager = groupManager ?? throw new ArgumentNullException(nameof(groupManager));
        }

        public int Run(SyncGroupOptions opts)
        {
            using (var group = m_GroupManager.OpenExclusively(opts.Group))
            {
                group.GetSynchronizer().Run();                
            }

            return 0;
        }
    }
}