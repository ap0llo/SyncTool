// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;
using SyncTool.FileSystem;
using SyncTool.FileSystem.Versioning;
using SyncTool.Git.Common;
using SyncTool.Synchronization.SyncActions;

namespace SyncTool.Git.FileSystem.Versioning
{
    public class GitBasedMultiFileSystemHistoryService : GitBasedService, IMultiFileSystemHistoryService
    {
        internal static readonly BranchName BranchName = new BranchName("MultiFileSystemSnapshots");        
        readonly IHistoryService m_HistoryService;

        public GitBasedMultiFileSystemHistoryService(GitBasedGroup group, IHistoryService historyService) : base(group)
        {
            if (historyService == null)            
                throw new ArgumentNullException(nameof(historyService));
            
            m_HistoryService = historyService;
        }


        public IMultiFileSystemSnapshot LatestSnapshot
        {
            get
            {
                if (!GitGroup.Repository.LocalBranchExists(BranchName))
                {
                    return null;
                }
                var tip = GitGroup.Repository.GetLocalBranch(BranchName).Tip;
                return Snapshots.FirstOrDefault(snapshot => snapshot.Id == tip.Sha);
            }
        }

        public IMultiFileSystemSnapshot this[string id]
        {
            get
            {
                if (String.IsNullOrWhiteSpace(id))
                    throw new ArgumentNullException(nameof(id));

                return GetSnapshot(id);
            }
        }

        public IEnumerable<IMultiFileSystemSnapshot> Snapshots
        {
            get
            {                
                if (!GitGroup.Repository.LocalBranchExists(BranchName))
                {
                    return Enumerable.Empty<IMultiFileSystemSnapshot>();
                }
                
                return GitGroup.Repository.GetLocalBranch(BranchName).Commits
                               .Where(GitBasedMultiFileSystemSnapshot.IsSnapshot)
                               .Select(commit => new GitBasedMultiFileSystemSnapshot(commit, m_HistoryService));
            }
        }

        public IMultiFileSystemSnapshot CreateSnapshot()
        {
            if (!GitGroup.Repository.LocalBranchExists(BranchName))
            {
                GitGroup.Repository.CreateBranch(BranchName, GitGroup.Repository.GetInitialCommit());
            }

            return GitBasedMultiFileSystemSnapshot.Create(GitGroup.Repository, BranchName, m_HistoryService);
        }

        public string[] GetChangedFiles(string to)
        {
            if (String.IsNullOrWhiteSpace(to))
                throw new ArgumentNullException(nameof(to));

            var result = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            var snapshot = GetSnapshot(to);
            foreach (var histoyName in snapshot.HistoyNames)
            {
                var history = m_HistoryService[histoyName];
                var snapshotId = snapshot.GetSnapshotId(histoyName);
                var changedFiles = history.GetChangedFiles(snapshotId);

                foreach (var filePath in changedFiles)
                {
                    result.Add(filePath);
                }
            }

            return result.ToArray();
        }

        public string[] GetChangedFiles(string from, string to)
        {
            if (String.IsNullOrWhiteSpace(from))
                throw new ArgumentNullException(nameof(to));

            if (String.IsNullOrWhiteSpace(to))
                throw new ArgumentNullException(nameof(to));



            var result = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            var fromSnapshot = GetSnapshot(from);
            var toSnapshot = GetSnapshot(to);

            foreach (var histoyName in fromSnapshot.HistoyNames)
            {
                var history = m_HistoryService[histoyName];                

                var changedFiles = history.GetChangedFiles(
                        fromSnapshot.GetSnapshotId(histoyName), 
                        toSnapshot.GetSnapshotId(histoyName)
                    );

                foreach (var filePath in changedFiles)
                {
                    result.Add(filePath);
                }
            }

            return result.ToArray();
        }


        IMultiFileSystemSnapshot GetSnapshot(string id)
        {
            
            var commit = GitGroup.Repository.Lookup<Commit>(id);
            if (commit == null || !GitBasedMultiFileSystemSnapshot.IsSnapshot(commit))
            {
                throw new SnapshotNotFoundException(id);
            }
            else
            {
                return new GitBasedMultiFileSystemSnapshot(commit, m_HistoryService);
            }
        }
    }
}