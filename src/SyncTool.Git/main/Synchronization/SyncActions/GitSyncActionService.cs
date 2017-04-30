using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SyncTool.FileSystem;
using SyncTool.FileSystem.Local;
using SyncTool.Git.Common;
using SyncTool.Git.FileSystem;
using SyncTool.Synchronization;
using SyncTool.Synchronization.SyncActions;
using Directory = SyncTool.FileSystem.Directory;

namespace SyncTool.Git.Synchronization.SyncActions
{
    public class GitSyncActionService : GitBasedService, ISyncActionService
    {
        internal static readonly BranchName BranchName = new BranchName("synchronization", "actions");
               


        public IEnumerable<SyncAction> AllItems
        {
            get
            {
                // branch does not exist => result is empty
                if (!Repository.Value.LocalBranchExists(BranchName))
                {
                    return Enumerable.Empty<SyncAction>();
                }

                // get actions for the file in any state
                var allStates = Enum.GetValues(typeof(SyncActionState)).Cast<SyncActionState>();
                return allStates.SelectMany(state => this[state]);
            }
        }

        public IEnumerable<SyncAction> PendingItems
        {
            get
            {
                // branch does not exist => result is empty
                if (!Repository.Value.LocalBranchExists(BranchName))
                {
                    return Enumerable.Empty<SyncAction>();
                }

                var pendingStates = Enum.GetValues(typeof(SyncActionState))
                    .Cast<SyncActionState>()
                    .Where(s => s.IsPendingState());

                return pendingStates.SelectMany(state => this[state]);
            }
        } 

        public IEnumerable<SyncAction> this[SyncActionState state]
        {
            get
            {
                // branch does not exist => result is empty
                if (!Repository.Value.LocalBranchExists(BranchName))
                {
                    return Enumerable.Empty<SyncAction>();
                }

                // load the root directory of the branch
                var gitDirectory = new GitDirectory(null, "root", Repository.Value.GetLocalBranch(BranchName).Tip);

                // determine if the directory for action with the specified state exists and load actions from it            
                var directoryPath = GetRelativeSyncActionDirectoryPath(state);
                if (!gitDirectory.DirectoryExists(directoryPath))
                {
                    return Enumerable.Empty<SyncAction>();
                }

                var directory = gitDirectory.GetDirectory(directoryPath);
                return LoadSyncActions(directory);
            }
        }

        public IEnumerable<SyncAction> this[SyncActionState state, string filePath]
        {
            get
            {
                PathValidator.EnsureIsValidFilePath(filePath);
                PathValidator.EnsureIsRootedPath(filePath);

                // branch does not exist => result is empty
                if (!Repository.Value.LocalBranchExists(BranchName))
                {
                    return Enumerable.Empty<SyncAction>();
                }

                // load the root directory of the branch
                var gitDirectory = new GitDirectory(null, "root", Repository.Value.GetLocalBranch(BranchName).Tip);

                // determine if the directory for action with the specified state and path exists and load actions from it            
                var directoryPath = GetRelativeSyncActionDirectoryPath(state, filePath);
                if (!gitDirectory.DirectoryExists(directoryPath))
                {
                    return Enumerable.Empty<SyncAction>();
                }
                else
                {
                    var directory = gitDirectory.GetDirectory(directoryPath);
                    return LoadSyncActions(directory);
                }
            }
        }

        public IEnumerable<SyncAction> this[string filePath]
        {
            get
            {
                PathValidator.EnsureIsValidFilePath(filePath);
                PathValidator.EnsureIsRootedPath(filePath);

                // branch does not exist => result is empty
                if (!Repository.Value.LocalBranchExists(BranchName))
                {
                    return Enumerable.Empty<SyncAction>();
                }

                // get actions for the file in any state
                var allStates = Enum.GetValues(typeof (SyncActionState)).Cast<SyncActionState>();
                return allStates.SelectMany(state => this[state, filePath]);
            }
        }



        public GitSyncActionService(GitRepository repository) : base(repository)
        {
        }


        public void AddItems(IEnumerable<SyncAction> syncActions)
        {
            syncActions = syncActions.ToArray();

            if (!syncActions.Any())
            {
                return;
            }

            // make sure, the action does not already exist
            foreach (var action in syncActions)
            {
                var exisitingActions = this[action.State, action.Path].Where(a => a.Id == action.Id);
                if (exisitingActions.Any())
                {
                    throw new DuplicateSyncActionException($"A sync action with id {action.Id} already exists");
                }
            }

            // create the branch if it does not already exist
            EnsureBranchExists();

            // create a file system tree for the actions
            var root = new Directory(null, "root");
            foreach (var syncAction in syncActions)
            {
                PathValidator.EnsureIsRootedPath(syncAction.Path);

                var directory = DirectoryHelper.GetOrAddDirectory(root, GetRelativeSyncActionDirectoryPath(syncAction));
                directory.Add(d => new SyncActionFile(d, syncAction));
            }


            // store the actions
            using (var workingDir = new TemporaryWorkingDirectory(Repository.Value.Info.Path, BranchName.ToString()))
            {               
                // create the actions on disk
                var localItemCreator = new LocalItemCreator();
                localItemCreator.CreateDirectoryInPlace(root, workingDir.Location);

                // commit (no check if there are pending changes, because there will always be changes (we made sure that syncActions is not empty and action do not yet exist))
                workingDir.Commit($"{nameof(GitSyncActionService)}: Added {syncActions.Count()} items");
                workingDir.Push();
            }
        }

        public void UpdateItems(IEnumerable<SyncAction> syncActions)
        {
            syncActions = syncActions.ToList();

            if (!syncActions.Any())
            {
                return;
            }

            // make sure all to be updated actions exist (no need to check the state, this property might have changed)
            AssertSyncActionsExist(syncActions, false);
            
            using (var workingDir = new TemporaryWorkingDirectory(Repository.Value.Info.Path, BranchName.ToString()))
            {
                var root = new Directory(null, "root");

                foreach (var syncAction in syncActions)
                {
                    PathValidator.EnsureIsRootedPath(syncAction.Path);

                    // remove existing files for this sync action
                    var filesToDelete = Enum.GetValues(typeof (SyncActionState)).Cast<SyncActionState>()
                        .Where(state => state != syncAction.State)
                        .Select(state => GetRelativeSyncActionDirectoryPath(state, syncAction.Path))
                        .Select(relativeDirectoryPath => Path.Combine(relativeDirectoryPath, SyncActionFile.GetFileName(syncAction)))
                        .Select(relativePath => Path.Combine(workingDir.Location, relativePath))
                        .Where(System.IO.File.Exists).ToList();

                    foreach (var file in filesToDelete)
                    {
                        System.IO.File.Delete(file);
                    }

                    // add a new file
                    var directory = DirectoryHelper.GetOrAddDirectory(root, GetRelativeSyncActionDirectoryPath(syncAction));
                    directory.Add(d => new SyncActionFile(d, syncAction));
                }

                var localItemCreator = new LocalItemCreator();
                localItemCreator.CreateDirectoryInPlace(root, workingDir.Location);

                workingDir.Commit($"{nameof(GitSyncActionService)}: Updated {syncActions.Count()} items");
                workingDir.Push();
            }
        }

        public void RemoveItems(IEnumerable<SyncAction> syncActions)
        {
            syncActions = syncActions.ToList();

            if (!syncActions.Any())
            {
                return;
            }

            // make sure all to be updated actions exist (otherwise we cannot remove them)
            AssertSyncActionsExist(syncActions, true);

            using (var workingDir = new TemporaryWorkingDirectory(Repository.Value.Info.Path, BranchName.ToString()))
            {
                var localDirectory = new LocalDirectory(null, workingDir.Location);

                // delete the file                 
                foreach (var syncAction in syncActions)
                {
                    PathValidator.EnsureIsRootedPath(syncAction.Path);

                    var directory = localDirectory.GetDirectory(GetRelativeSyncActionDirectoryPath(syncAction));
                    var file = (ILocalFile) directory.GetFile(SyncActionFile.GetFileName(syncAction));
                    System.IO.File.Delete(file.Location);
                }

                workingDir.Commit($"{nameof(GitSyncActionService)}: Removed {syncActions.Count()} items");
                workingDir.Push();
            }
        }


        /// <summary>
        /// Creates the sync action branch in the underlying repository if it does not exist yet
        /// </summary>
        void EnsureBranchExists()
        {
            if (!Repository.Value.LocalBranchExists(BranchName))
            {
                var initalCommit = Repository.Value.GetInitialCommit();
                Repository.Value.CreateBranch(BranchName, initalCommit);
            }
        }

        /// <summary>
        /// Checks that all the specified sync actions already exist and throws a <see cref="SyncActionNotFoundException"/> if the action can not be found
        /// </summary>
        /// <param name="syncActions">The action to check</param>
        /// <param name="checkState">Specifies whether to also check if the existing action have the same state value as the specified actions</param>
        void AssertSyncActionsExist(IEnumerable<SyncAction> syncActions, bool checkState)
        {
            if (checkState)
            {
                foreach (var action in syncActions)
                {
                    var exisitingActions = this[action.State, action.Path].Where(a => a.Id == action.Id);
                    if (!exisitingActions.Any())
                    {
                        throw new SyncActionNotFoundException($"A sync action with id {action.Id} and state {action.State} was not found and cannot be updated");
                    }
                }
            }
            else
            {
                foreach (var action in syncActions)
                {
                    var exisitingActions = this[action.Path].Where(a => a.Id == action.Id);
                    if (!exisitingActions.Any())
                    {
                        throw new SyncActionNotFoundException($"A sync action with id {action.Id} was not found and cannot be updated");
                    }
                }
            }
        }

        /// <summary>
        /// Recursively loads all sync actions from the specified directory
        /// </summary>
        IEnumerable<SyncAction> LoadSyncActions(IDirectory directory)
        {
            return directory
                .EnumerateFilesRecursively()
                .Where(f => f.Name.EndsWith(SyncActionFile.FileNameSuffix, StringComparison.InvariantCultureIgnoreCase))
                .Cast<IReadableFile>()
                .Select(file => SyncActionFile.Load(null, file).Content);
        }



        string GetRelativeSyncActionDirectoryPath(SyncAction action) => GetRelativeSyncActionDirectoryPath(action.State, action.Path);

        string GetRelativeSyncActionDirectoryPath(SyncActionState state, string filePath) => state + filePath;

        string GetRelativeSyncActionDirectoryPath(SyncActionState state) => state.ToString();
    }
}