using System;
using System.Linq;
using CommandLine;
using SyncTool.Cli.Framework;
using SyncTool.Cli.Output;
using SyncTool.Common;
using SyncTool.Configuration.Model;
using SyncTool.FileSystem.Versioning;

namespace SyncTool.Cli.Commands
{
    [Verb("Get-Snapshot")]
    public class GetSnapshotOptions : OptionsBase
    {
        [Option('g', "group", Required = true)]
        public string Group { get; set; }

        [Option('f', "folder", Required = true)]
        public string Folder { get; set; }

    }


    public class GetSnapshotCommand : CommandBase, ICommand<GetSnapshotOptions>
    {
        readonly IGroupManager m_GroupManager;
       

        public GetSnapshotCommand(IOutputWriter outputWriter, IGroupManager groupManager) : base(outputWriter)
        {
            if (groupManager == null)
            {
                throw new ArgumentNullException(nameof(groupManager));
            }         
            m_GroupManager = groupManager;
        }


        public int Run(GetSnapshotOptions opts)
        {
            using (var group = m_GroupManager.GetGroup(opts.Group))
            {
                var configurationService = group.GetService<IConfigurationService>();
                var historyService = group.GetService<IHistoryService>();

                var configuration = configurationService[opts.Folder];
                var history = historyService[opts.Folder];

                OutputWriter.WriteLine($"SyncGroup '{group.Name}', Folder '{configuration.Name}'");                
                OutputWriter.WriteLine();
                PrintHistory(history);
            }
            return 0;


        }
        

        void PrintHistory(IFileSystemHistory history)
        {
            if (history != null && history.Snapshots.Any())
            {
                OutputWriter.WriteTable(
                    new[]
                    {
                        "Id",
                        "CreationTime"
                    }, 
                    new []
                    {
                        history.Snapshots.Select(x => x.Id),
                        history.Snapshots.Select(x => x.CreationTime.ToString())
                    });                
            }
            else
            {
                OutputWriter.WriteLine("\tNo snapshots found");
            }
        }        

    }
}