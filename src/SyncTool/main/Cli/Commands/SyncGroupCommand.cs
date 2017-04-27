using System;
using System.Linq;
using CommandLine;
using SyncTool.Cli.Framework;
using SyncTool.Cli.Output;
using SyncTool.Common;
using SyncTool.FileSystem;
using SyncTool.FileSystem.Versioning;
using SyncTool.Synchronization;
using SyncTool.Synchronization.SyncActions;
using SyncTool.Synchronization.State;

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
        readonly ISynchronizer m_Synchronizer;

        public SyncGroupCommand(IOutputWriter outputWriter, IGroupManager groupManager, ISynchronizer synchronizer) : base(outputWriter)
        {
            m_GroupManager = groupManager ?? throw new ArgumentNullException(nameof(groupManager));
            m_Synchronizer = synchronizer ?? throw new ArgumentNullException(nameof(synchronizer));
        }


        public int Run(SyncGroupOptions opts)
        {
            using (var group = m_GroupManager.GetGroup(opts.Group))
            {            
                m_Synchronizer.Synchronize(group);
            }

            return 0;
        }

     

    }
}