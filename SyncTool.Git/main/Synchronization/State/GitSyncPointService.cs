// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using SyncTool.FileSystem;
using SyncTool.FileSystem.Local;
using SyncTool.Git.Common;
using SyncTool.Git.FileSystem;
using SyncTool.Synchronization.State;

namespace SyncTool.Git.Synchronization.State
{
    public class GitSyncPointService : GitBasedService, ISyncPointService
    {
        internal static readonly BranchName BranchName = new BranchName("synchronization", "state");
        const string s_DirectoryName = "SyncPoints";
        

        public IEnumerable<ISyncPoint> Items
        {
            get
            {
                if (!GitGroup.Repository.LocalBranchExists(BranchName))
                {
                    return Enumerable.Empty<ISyncPoint>();
                }

                var root = new GitDirectory(null, "root", GitGroup.Repository.GetLocalBranch(BranchName).Tip);

                if (!root.DirectoryExists(s_DirectoryName))
                {
                    return Enumerable.Empty<ISyncPoint>();
                }

                return LoadSynchronizationStates(root.GetDirectory(s_DirectoryName));
            }
        }

        public ISyncPoint this[int id]
        {
            get
            {
                if (!GitGroup.Repository.LocalBranchExists(BranchName))
                {
                    throw new SyncPointNotFoundException(id);
                }

                var root = new GitDirectory(null, "root", GitGroup.Repository.GetLocalBranch(BranchName).Tip);
                var relativePath = GetRelativeSynchronizationStateFilePath(id);

                if (!root.FileExists(relativePath))
                {
                    throw new SyncPointNotFoundException(id);
                }

                return SynchronizationStateFile.Load(null, (IReadableFile)root.GetFile(relativePath)).Content;
            }
        }


        public GitSyncPointService(GitBasedGroup gitGroup) : base(gitGroup)
        {
       

        }


        public bool ItemExists(int id)
        {
            if (!GitGroup.Repository.LocalBranchExists(BranchName))
            {
                return false;
            }

            var root = new GitDirectory(null, "root", GitGroup.Repository.GetLocalBranch(BranchName).Tip);
            var relativePath = GetRelativeSynchronizationStateFilePath(id);

            return root.FileExists(relativePath);
        }

        public void AddItem(ISyncPoint state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }
            
            if (ItemExists(state.Id))
            {
                throw new DuplicateSyncPointException(state.Id);
            }

            // create synchronization state branch if necessary
            EnsureBranchExists();

            var directory = new Directory(null, s_DirectoryName)
            {
                d => new SynchronizationStateFile(d, state)
            };

            using (var workingDirectory = new TemporaryWorkingDirectory(GitGroup.Repository.Info.Path, BranchName.ToString()))
            {
                var localItemCreator = new LocalItemCreator();
                localItemCreator.CreateDirectory(directory, workingDirectory.Location);

                workingDirectory.Commit($"{nameof(GitSyncPointService)}: Added SynchronizationState {state.Id}");
                workingDirectory.Push();
            }            
        }


        IEnumerable<ISyncPoint> LoadSynchronizationStates(IDirectory directory)
        {
            return directory
               .EnumerateFilesRecursively()
               .Where(f => f.Name.EndsWith(SynchronizationStateFile.FileNameSuffix, StringComparison.InvariantCultureIgnoreCase))
               .Cast<IReadableFile>()
               .Select(file => SynchronizationStateFile.Load(null, file).Content);
        }

        void EnsureBranchExists()
        {
            if (!GitGroup.Repository.LocalBranchExists(BranchName))
            {
                GitGroup.Repository.CreateBranch(BranchName, GitGroup.Repository.GetInitialCommit());
            }
        }

        string GetRelativeSynchronizationStateFilePath(int id)
        {            
            return s_DirectoryName + "/" + SynchronizationStateFile.GetFileName(id);
        }
    }
}