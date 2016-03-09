// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using SyncTool.FileSystem;
using SyncTool.FileSystem.Local;
using SyncTool.Git.Common;
using SyncTool.Git.FileSystem;
using SyncTool.Synchronization.SyncActions;
using SyncTool.Synchronization.Transfer;
using Directory = SyncTool.FileSystem.Directory;
using NativeDirectory = System.IO.Directory;


namespace SyncTool.Git.Synchronization.Transfer
{
    public class GitSynchronizationState : ISynchronizationState
    {

        public const string BranchNamePrefix = "synchronizationState";

        const string s_Queued = "Queued";
        const string s_InProgress = "InProgress";
        const string s_Completed = "Completed";


        readonly BranchName m_BranchName;
        readonly Repository m_Repository;
        readonly SyncActionSerializer m_Serializer = new SyncActionSerializer();

        readonly Lazy<GitDirectory> m_GitDirectory; 
        readonly Lazy<List<SyncAction>> m_QueuedActions;
        readonly Lazy<List<SyncAction>> m_InProgressActions;
        readonly Lazy<List<SyncAction>> m_CompletedActions;
        readonly Lazy<SynchronizationStateSnapshotIds> m_SnapshotIds;


        public string LocalSnapshotId => m_SnapshotIds.Value.LocalSnapshotId;

        public string GlobalSnapshotId => m_SnapshotIds.Value.GlobalSnapshotId;

        public IEnumerable<SyncAction> QueuedActions => m_QueuedActions.Value;

        public IEnumerable<SyncAction> InProgressActions => m_InProgressActions.Value;

        public IEnumerable<SyncAction> CompletedActions => m_CompletedActions.Value;



        public GitSynchronizationState(Repository repository, BranchName branchName)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }
            if (branchName == null)
            {
                throw new ArgumentNullException(nameof(branchName));
            }
            m_Repository = repository;
            m_BranchName = branchName;

            m_QueuedActions = new Lazy<List<SyncAction>>(LoadQueuedActions);
            m_InProgressActions = new Lazy<List<SyncAction>>(LoadInProgressActions);
            m_CompletedActions = new Lazy<List<SyncAction>>(LoadCompletedActions);

            m_GitDirectory = new Lazy<GitDirectory>(() =>
            {
                var branch = m_Repository.GetBranch(m_BranchName);
                return new GitDirectory(null, "root", branch.Tip);
            });

            m_SnapshotIds = new Lazy<SynchronizationStateSnapshotIds>(LoadSnapshotIds);
        }



        public static GitSynchronizationState Create(Repository repository, BranchName branchName, ISynchronizationState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }            

            var ids = new SynchronizationStateSnapshotIds()
            {
                LocalSnapshotId = state.LocalSnapshotId,
                GlobalSnapshotId = state.GlobalSnapshotId
            };
            
            var fileSystem = new Directory("root")
            {
                root => new SynchronizationStateSnapshotIdsFile(root, ids),
                root => GetSyncActionDirectory(root, s_Queued, state.QueuedActions),
                root => GetSyncActionDirectory(root, s_InProgress, state.InProgressActions),
                root => GetSyncActionDirectory(root, s_Completed, state.CompletedActions)
            };

            using (var workingDirectory = new TemporaryWorkingDirectory(repository.Info.Path, branchName.ToString()))
            {
                var localItemCreator = new LocalItemCreator();

                foreach(var name in new[] { s_Queued, s_InProgress, s_Completed })
                {
                    var path = Path.Combine(workingDirectory.Location, name);
                    if (NativeDirectory.Exists(path))
                    {
                        NativeDirectory.Delete(path: path, recursive: true);
                    }
                }
                
                localItemCreator.CreateDirectoryInPlace(fileSystem, workingDirectory.Location);

                if (workingDirectory.HasChanges)
                {
                    workingDirectory.Commit($"Updated synchronization state '{branchName}'");
                    workingDirectory.Push();
                }
            }

            return new GitSynchronizationState(repository, branchName);
        }

        List<SyncAction> LoadQueuedActions() => LoadSyncActions(s_Queued);
        
        List<SyncAction> LoadInProgressActions() => LoadSyncActions(s_InProgress);

        List<SyncAction> LoadCompletedActions() => LoadSyncActions(s_Completed);
        

        List<SyncAction> LoadSyncActions(string directoryName)
        {
            var dir = m_GitDirectory.Value;
            if (dir.DirectoryExists(directoryName))
            {
                return LoadSyncActions(dir.GetDirectory(directoryName));
            }
            else
            {
                return new List<SyncAction>();
            }
        }

        List<SyncAction> LoadSyncActions(IDirectory directory)
        {
            var files = directory.Files
                .Where(f => f.Name.EndsWith(SyncActionFile.FileSuffix, StringComparison.CurrentCultureIgnoreCase))
                .Cast<IReadableFile>();

            var actions = new List<SyncAction>();

            foreach (var file in files)
            {
                using (var stream = file.OpenRead())
                {
                    var action = m_Serializer.Deserialize(stream);
                    actions.Add(action);
                }
            }

            return actions;
        }

        SynchronizationStateSnapshotIds LoadSnapshotIds()
        {
            var file = (IReadableFile) m_GitDirectory.Value.GetFile(SynchronizationStateSnapshotIdsFile.FileName);
            using (var stream = file.OpenRead())
            {
                var ids = stream.Deserialize<SynchronizationStateSnapshotIds>();
                return ids;
            }
        }


     
        static Directory GetSyncActionDirectory(IDirectory parent, string name, IEnumerable<SyncAction> actions)
        {
            var directory = new Directory(parent, name);
            
            foreach (var action in actions)
            {
                directory.Add(d => new SyncActionFile(d, action));
            }
            return directory;
        }

    }
}