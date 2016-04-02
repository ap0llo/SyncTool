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
    public class GitSynchronizationStateService : GitBasedService, ISynchronizationStateService
    {
        static readonly BranchName s_BranchName = new BranchName("synchronization", "state");
        const string s_DirectoryName = "SynchronizationState";
                
        readonly Lazy<IDictionary<int, ISynchronizationState>> m_Items;


        public IEnumerable<ISynchronizationState> Items => m_Items.Value.Values;

        public ISynchronizationState this[int id]
        {
            get
            {
                if (!m_Items.Value.ContainsKey(id))
                {
                    throw new SynchronizationStateNotFoundException(id);
                }
                return m_Items.Value[id];
            }
        }


        public GitSynchronizationStateService(GitBasedGroup gitGroup) : base(gitGroup)
        {
            m_Items = new Lazy<IDictionary<int, ISynchronizationState>>(LoadItems);
        }


        public bool ItemExists(int key) => m_Items.Value.ContainsKey(key);

        public void AddSynchronizationState(ISynchronizationState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (ItemExists(state.Id))
            {
                throw new DuplicateSynchronizationStateException(state.Id);
            }

            // create synchronization state branch if neccessary
            if (!GitGroup.Repository.LocalBranchExists(s_BranchName))
            {
                GitGroup.Repository.CreateBranch(s_BranchName, GitGroup.Repository.GetInitialCommit());
            }

            var directory = new Directory(null,s_DirectoryName)
            {
                d => new SynchronizationStateFile(d, state)
            };

            using (var workingDirectory = new TemporaryWorkingDirectory(GitGroup.Repository.Info.Path, s_BranchName.ToString()))
            {
                var localItemCreator = new LocalItemCreator();
                localItemCreator.CreateDirectory(directory, workingDirectory.Location);

                workingDirectory.Commit($"Added SynchronizationState {state.Id}");
                workingDirectory.Push();
            }

            m_Items.Value.Add(state.Id, state);
        }


        IDictionary<int, ISynchronizationState> LoadItems()
        {
            if (!GitGroup.Repository.LocalBranchExists(s_BranchName))
            {                
                return new Dictionary<int, ISynchronizationState>();
            }
            var directory = new GitDirectory(null, "root", GitGroup.Repository.GetLocalBranch(s_BranchName).Tip);

            if (!directory.DirectoryExists(s_DirectoryName))
            {
                return new Dictionary<int, ISynchronizationState>();
            }

            return LoadItems(directory.GetDirectory(s_DirectoryName));
        }

        IDictionary<int, ISynchronizationState> LoadItems(IDirectory directory)
        {
            var parent = new Directory("root");
            var result = new Dictionary<int, ISynchronizationState>();

            foreach (var file in directory.Files.Where(f => f.Name.EndsWith(SynchronizationStateFile.FileNameSuffix)).Cast<IReadableFile>())
            {
                var stateFile = SynchronizationStateFile.Load(parent, file);
                result.Add(stateFile.Content.Id, stateFile.Content);
            }

            return result;
        }

    }
}