// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

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
using SyncTool.Synchronization.Transfer;

namespace SyncTool.Cli.Commands
{    

    [Verb("Synchronize")]
    public class SynchronizeOptions
    {
        [Option('g', "group", Required = true)]
        public string Group { get; set; }

        [Option('f', "folder", Required = true)]
        public string Folder { get; set; }

    }

    public class SynchronizeCommand : CommandBase, ICommand<SynchronizeOptions>
    {
        //TODO: Ensure this name cannot be used as syncfolder name
        const string s_Global = "Global";

        readonly IGroupManager m_GroupManager;
        readonly ISynchronizer m_Synchronizer;

        public SynchronizeCommand(IOutputWriter outputWriter, IGroupManager groupManager, ISynchronizer synchronizer) : base(outputWriter)
        {
            if (groupManager == null)
            {
                throw new ArgumentNullException(nameof(groupManager));
            }
            if (synchronizer == null)
            {
                throw new ArgumentNullException(nameof(synchronizer));
            }
            m_GroupManager = groupManager;
            m_Synchronizer = synchronizer;
        }


        public int Run(SynchronizeOptions opts)
        {
            using (var group = m_GroupManager.GetGroup(opts.Group))
            {
                var historyService = group.GetHistoryService();
                var synchronizationStateService = group.GetSynchronizationStateService();

                var localHistory = historyService[opts.Folder];
                if (!localHistory.Snapshots.Any())
                {
                    OutputWriter.WriteErrorLine($"No snapshots for '{opts.Folder}' found");
                    return 1;
                }

                var globalHistory = GetGlobalHistory(historyService);
                
                // get the synchronization state for the folder
                var synchronizationState = GetSynchronizationState(synchronizationStateService, opts.Folder, globalHistory, localHistory);

                if (!synchronizationState.IsCompleted())
                {
                    OutputWriter.WriteErrorLine($"There are still pending action from the last synchronization. Wait until these are finished");
                    return 1;
                }
            
                var globalChanges = globalHistory.CompareSnapshots(synchronizationState.GlobalSnapshotId, globalHistory.LatestFileSystemSnapshot.Id);
                var localChanges = localHistory.CompareSnapshots(synchronizationState.LocalSnapshotId, localHistory.LatestFileSystemSnapshot.Id);

                var syncResult = m_Synchronizer.Synchronize(globalChanges, localChanges);

                if (syncResult.Conflicts.Any())
                {
                    throw new NotImplementedException();
                }

                // create a new global snapshot                
                var globalState = syncResult.ApplyTo(globalHistory.LatestFileSystemSnapshot.RootDirectory, SyncParticipant.Left);
                globalHistory.CreateSnapshot(globalState);

                // create new sync state
                var localActions = syncResult.Actions.Where(action => action.Target == SyncParticipant.Right);
                
                synchronizationStateService[opts.Folder] = new MutableSynchronizationState()
                {
                    LocalSnapshotId = localChanges.ToSnapshot.Id,
                    GlobalSnapshotId = globalChanges.ToSnapshot.Id,
                    QueuedActions = synchronizationState.QueuedActions.Union(localActions).ToList(),
                    InProgressActions = synchronizationState.InProgressActions,
                    CompletedActions = synchronizationState.CompletedActions
                };

                return 0;

            }            
        }

        IFileSystemHistory GetGlobalHistory(IHistoryService historyService)
        {
            if (historyService.ItemExists(s_Global))
            {
                return historyService[s_Global];
            }

            historyService.CreateHistory(s_Global);
            var globalHistory = historyService[s_Global];
            globalHistory.CreateSnapshot(new Directory(historyService.Group.Name));

            return globalHistory;
        }

        ISynchronizationState GetSynchronizationState(ISynchronizationStateService stateService, string name, IFileSystemHistory globalHistory, IFileSystemHistory localHistory)
        {            
            return stateService.ItemExists(name)
                ? stateService[name]
                : new MutableSynchronizationState()
                {
                    GlobalSnapshotId = globalHistory.GetOldestSnapshot().Id,
                    LocalSnapshotId = localHistory.GetOldestSnapshot().Id
                };            
        }

    }
}