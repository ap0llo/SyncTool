using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LibGit2Sharp;
using SyncTool.FileSystem.Versioning;
using SyncTool.Git.RepositoryAccess;

namespace SyncTool.Git.FileSystem.Versioning
{
    public class GitBasedMultiFileSystemHistoryService : AbstractMultiFileSystemHistoryService
    {
        internal static readonly BranchName BranchName = new BranchName("MultiFileSystemSnapshots");        

        readonly GitRepository m_Repository;
        readonly WorkingDirectoryFactory m_WorkingDirectoryFactory;
        readonly IHistoryService m_HistoryService;


        public override IMultiFileSystemSnapshot LatestSnapshot
        {
            get
            {
                if (!m_Repository.Value.LocalBranchExists(BranchName))
                {
                    return null;
                }
                var tip = m_Repository.Value.GetLocalBranch(BranchName).Tip;
                return Snapshots.FirstOrDefault(snapshot => snapshot.Id == tip.Sha);
            }
        }

        public override IEnumerable<IMultiFileSystemSnapshot> Snapshots
        {
            get
            {
                if (!m_Repository.Value.LocalBranchExists(BranchName))
                {
                    return Enumerable.Empty<IMultiFileSystemSnapshot>();
                }

                return m_Repository.Value.GetLocalBranch(BranchName).Commits
                    .Where(GitBasedMultiFileSystemSnapshot.IsSnapshot)
                    .Select(commit => new GitBasedMultiFileSystemSnapshot(commit, m_HistoryService));
            }
        }


        public GitBasedMultiFileSystemHistoryService(
            [NotNull] GitRepository repository, 
            [NotNull] WorkingDirectoryFactory workingDirectoryFactory, 
            [NotNull] IHistoryService historyService) : base(historyService)
        {
            m_Repository = repository ?? throw new ArgumentNullException(nameof(repository));
            m_WorkingDirectoryFactory = workingDirectoryFactory ?? throw new ArgumentNullException(nameof(workingDirectoryFactory));
            m_HistoryService = historyService ?? throw new ArgumentNullException(nameof(historyService));
            m_HistoryService = historyService ?? throw new ArgumentNullException(nameof(historyService));
        }

        
        public override IMultiFileSystemSnapshot CreateSnapshot()
        {
            if (!m_Repository.Value.LocalBranchExists(BranchName))
            {
                m_Repository.Value.CreateBranch(BranchName, m_Repository.Value.GetInitialCommit());
            }

            return GitBasedMultiFileSystemSnapshot.Create(m_WorkingDirectoryFactory, m_Repository.Value, BranchName, m_HistoryService);
        }
        

        protected override IMultiFileSystemSnapshot GetSnapshot(string id)
        {            
            var commit = m_Repository.Value.Lookup<Commit>(id);
            if (commit == null || !GitBasedMultiFileSystemSnapshot.IsSnapshot(commit))
            {
                throw new SnapshotNotFoundException(id);
            }
            else
            {
                return new GitBasedMultiFileSystemSnapshot(commit, m_HistoryService);
            }
        }

        protected override void AssertIsAncestor(string ancestorId, string descandantId)
        {
            if (!m_Repository.Value.IsCommitAncestor(ancestorId, descandantId))
            {
                throw new InvalidRangeException($"Snapshot {descandantId} is not an descendant of {ancestorId}");
            }
        }
    }
}