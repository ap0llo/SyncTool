// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
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



        IMultiFileSystemSnapshot LoadSnapshot(IDirectory directory)
        {
            throw new NotImplementedException();
        }
    }
}