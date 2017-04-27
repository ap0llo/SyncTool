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
    public class AddFolderOptions : OptionsBase
    {
        [Option('g', "group", Required = true)]
        public string Group { get; set; }

        [Option('n', "name", Required = true)]
        public string Name { get; set; }

        [Option('p', "path", Required = true)]
        public string Path { get; set; }                
    }



    public class AddFolderCommand : CommandBase, ICommand<AddFolderOptions>
    {
        readonly IGroupManager m_GroupManager;
        

        public AddFolderCommand(IOutputWriter outputWriter, IGroupManager groupManager) : base(outputWriter)
        {
            m_GroupManager = groupManager ?? throw new ArgumentNullException(nameof(groupManager));            
        }


        public int Run(AddFolderOptions opts)
        {            
            using (var group = m_GroupManager.GetGroup(opts.Group))            
            {                
                var configService = group.GetService<IConfigurationService>();
                configService.AddItem(new SyncFolder(opts.Name) { Path = opts.Path });

                var historyService = group.GetService<IHistoryService>();
                historyService.CreateHistory(opts.Name);
            }
            return 0;
        }
    }
}