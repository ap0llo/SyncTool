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

    [Verb("Sync")]
    public class SyncOptions
    {
        [Option('g', "group", Required = true)]
        public string Group { get; set; }

        [Option('f', "folder", Required = true)]
        public string Folder { get; set; }

    }

    public class SyncCommand : CommandBase, ICommand<SyncOptions>
    {
        //TODO: Ensure this name cannot be used as syncfolder name
        const string s_Global = "Global";

        readonly IGroupManager m_GroupManager;
        readonly ISynchronizer m_Synchronizer;

        public SyncCommand(IOutputWriter outputWriter, IGroupManager groupManager, ISynchronizer synchronizer) : base(outputWriter)
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


        public int Run(SyncOptions opts)
        {
            using (var group = m_GroupManager.GetGroup(opts.Group))
            {
                // get required services
                var historyService = group.GetHistoryService();
                var synchronizationStateService = group.GetSynchronizationStateService();
          
                // get the synchronization state for the folder
                var synchronizationState = GetSynchronizationState(synchronizationStateService, opts.Folder);
                
                // check if a sync can be run (all previous sync actions need to be completed)
                if (!CanRunSynchronization(synchronizationState))
                {                    
                    return 1;
                }

                // get global and local histories
                var globalHistory = GetGlobalHistory(historyService);
                var localHistory = historyService[opts.Folder];
                if (!CanRunSynchronization(localHistory))
                {                    
                    return 1;
                }

                throw new NotImplementedException();

//
//                // get global and local changes
//                var globalChanges = GetGlobalChanges(synchronizationStateService, globalHistory, opts.Folder);
//                var localChanges = GetLocalChanges(synchronizationStateService, localHistory, opts.Folder);
//
//                // synchronize
//                var syncResult = m_Synchronizer.Synchronize(globalChanges, localChanges);
//
//                // handle sync conflicts
//                syncResult = HandleSyncConflicts(syncResult);
//                
//                // create a new global snapshot                
//                var globalState = syncResult.ApplyTo(globalHistory.LatestFileSystemSnapshot.RootDirectory, SyncParticipant.Left);
//                globalHistory.CreateSnapshot(globalState);
//
//                // create and sace new sync state
//                var localActions = syncResult.Actions.Where(action => action.Target == SyncParticipant.Right);
//                
//                synchronizationStateService[opts.Folder] = new MutableSynchronizationState()
//                {
//                    LocalSnapshotId = localChanges.ToSnapshot.Id,
//                    GlobalSnapshotId = globalChanges.ToSnapshot.Id,
//                    QueuedActions = synchronizationState.QueuedActions.Union(localActions).ToList(),
//                    InProgressActions = synchronizationState.InProgressActions,
//                    CompletedActions = synchronizationState.CompletedActions
//                };
//
//                return 0;
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

        IFileSystemDiff GetLocalChanges(ISynchronizationStateService stateService, IFileSystemHistory localHistory, string stateName)
        {            
            if (stateService.ItemExists(stateName))
            {
                return localHistory.GetChanges(stateService[stateName].LocalSnapshotId, localHistory.LatestFileSystemSnapshot.Id);
            }
            else
            {
                return localHistory.GetChanges(localHistory.LatestFileSystemSnapshot.Id);
            }
        }

        IFileSystemDiff GetGlobalChanges(ISynchronizationStateService stateService, IFileSystemHistory globalHistory, string stateName)
        {
            if (stateService.ItemExists(stateName))
            {
                return globalHistory.GetChanges(stateService[stateName].GlobalSnapshotId, globalHistory.LatestFileSystemSnapshot.Id);
            }
            else
            {
                return globalHistory.GetChanges(globalHistory.LatestFileSystemSnapshot.Id);
            }
        }

        ISynchronizationState GetSynchronizationState(ISynchronizationStateService stateService, string name)
        {            
            return stateService.ItemExists(name)
                ? stateService[name]
                : new MutableSynchronizationState();            
        }

        bool CanRunSynchronization(ISynchronizationState state)
        {
            if (!state.IsCompleted())
            {
                OutputWriter.WriteErrorLine($"There are still pending action from the last synchronization. Wait until these are finished");
                return false;
            }
            else
            {
                return true;
            }
        }

        bool CanRunSynchronization(IFileSystemHistory localHistory)
        {
            if (!localHistory.Snapshots.Any())
            {
                OutputWriter.WriteErrorLine($"No snapshots for '{localHistory.Name}' found");
                return false;
            }
            return true;
        }

        ISynchronizerResult HandleSyncConflicts(ISynchronizerResult syncResult)
        {
            if (syncResult.Conflicts.Any())
            {
                throw new NotImplementedException();
            }
            return syncResult;
        }

    }
}