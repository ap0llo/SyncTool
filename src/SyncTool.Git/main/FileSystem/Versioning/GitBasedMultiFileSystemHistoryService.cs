using System;
using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;
using SyncTool.FileSystem.Versioning;
using SyncTool.Git.Common.Services;
using SyncTool.Git.RepositoryAccess;

namespace SyncTool.Git.FileSystem.Versioning
{
    public class GitBasedMultiFileSystemHistoryService : GitBasedService, IMultiFileSystemHistoryService
    {
        internal static readonly BranchName BranchName = new BranchName("MultiFileSystemSnapshots");        
        readonly IHistoryService m_HistoryService;


        public GitBasedMultiFileSystemHistoryService(GitRepository repository, IHistoryService historyService) : base(repository)
        {
            m_HistoryService = historyService ?? throw new ArgumentNullException(nameof(historyService));
        }


        public IMultiFileSystemSnapshot LatestSnapshot
        {
            get
            {
                if (!Repository.Value.LocalBranchExists(BranchName))
                {
                    return null;
                }
                var tip = Repository.Value.GetLocalBranch(BranchName).Tip;
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
                if (!Repository.Value.LocalBranchExists(BranchName))
                {
                    return Enumerable.Empty<IMultiFileSystemSnapshot>();
                }
                
                return Repository.Value.GetLocalBranch(BranchName).Commits
                               .Where(GitBasedMultiFileSystemSnapshot.IsSnapshot)
                               .Select(commit => new GitBasedMultiFileSystemSnapshot(commit, m_HistoryService));
            }
        }

        public IMultiFileSystemSnapshot CreateSnapshot()
        {
            if (!Repository.Value.LocalBranchExists(BranchName))
            {
                Repository.Value.CreateBranch(BranchName, Repository.Value.GetInitialCommit());
            }

            return GitBasedMultiFileSystemSnapshot.Create(Repository.Value, BranchName, m_HistoryService);
        }

        public string[] GetChangedFiles(string toId)
        {
            if (String.IsNullOrWhiteSpace(toId))
                throw new ArgumentNullException(nameof(toId));

            var result = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            var snapshot = GetSnapshot(toId);
            foreach (var histoyName in snapshot.HistoryNames)
            {
                var history = m_HistoryService[histoyName];                
                var changedFiles = history.GetChangedFiles(snapshot.GetSnapshotId(histoyName));

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
                throw new ArgumentNullException(nameof(from));

            if (String.IsNullOrWhiteSpace(to))
                throw new ArgumentNullException(nameof(to));

            var fromSnapshot = GetSnapshot(from);
            var toSnapshot = GetSnapshot(to);

            AssertIsAncestor(from, to);

            var result = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            
            foreach (var histoyName in toSnapshot.HistoryNames)
            {
                var history = m_HistoryService[histoyName];

                string[] changedFiles;
                // if the history already existed at the time of fromSnapshot, 
                // only get the changes between the snapshots                
                if (fromSnapshot.HistoryNames.Contains(histoyName))
                {
                    changedFiles = history.GetChangedFiles(
                        fromSnapshot.GetSnapshotId(histoyName), 
                        toSnapshot.GetSnapshotId(histoyName));
                }
                // if the current history did not exist at the time of fromSnapshot, 
                // get all changes for the current history
                else
                {
                    changedFiles = history.GetChangedFiles(toSnapshot.GetSnapshotId(histoyName));
                }

                foreach (var filePath in changedFiles)
                {
                    result.Add(filePath);
                }
            }

            return result.ToArray();
        }

        public IMultiFileSystemDiff GetChanges(string toId, string[] pathFilter = null)
        {
            if(String.IsNullOrWhiteSpace(toId))
                throw new ArgumentNullException(nameof(toId));

            var snapshot = GetSnapshot(toId);

            // get all change lists from all histories
            var diffs = snapshot.HistoryNames
                .Select(name => m_HistoryService[name].GetChanges(snapshot.GetSnapshotId(name)))
                .Select(diff => new Tuple<string, IFileSystemDiff>(diff.History.Name, diff));
            
            var fileChanges = CombineChangeLists(snapshot.HistoryNames.ToArray(), diffs);
                          
            // since we're getting all changes up to the specified snapshot,
            // all histories were added (initially there were none)
            var historyChanges = snapshot.HistoryNames.Select(name => new HistoryChange(name, ChangeType.Added));

            return new MultiFileSystemDiff(snapshot, fileChanges, historyChanges);
        }

        public IMultiFileSystemDiff GetChanges(string fromId, string toId, string[] pathFilter = null)
        {
            if (String.IsNullOrWhiteSpace(fromId))
                throw new ArgumentNullException(nameof(fromId));

            if (String.IsNullOrWhiteSpace(toId))
                throw new ArgumentNullException(nameof(toId));

            var fromSnapshot = GetSnapshot(fromId);
            var toSnapshot = GetSnapshot(toId);

            AssertIsAncestor(fromId, toId);

            // get file changes

            var diffs = new List<IFileSystemDiff>();
            foreach (var histoyName in toSnapshot.HistoryNames)
            {
                var history = m_HistoryService[histoyName];

                IFileSystemDiff diff;
                // if the history already existed at the time of fromSnapshot, 
                // only get the changes between the snapshots                
                if (fromSnapshot.HistoryNames.Contains(histoyName))
                {
                    diff = history.GetChanges(
                        fromSnapshot.GetSnapshotId(histoyName),
                        toSnapshot.GetSnapshotId(histoyName));
                }
                // if the current history did not exist at the time of fromSnapshot, 
                // get all changes for the current history
                else
                {
                    diff = history.GetChanges(toSnapshot.GetSnapshotId(histoyName));
                }

                diffs.Add(diff);
            }

            var fileChanges = CombineChangeLists(
                toSnapshot.HistoryNames.ToArray(), 
                diffs.Select(d => new Tuple<string, IFileSystemDiff>(d.History.Name, d))
            );

            // get history changes

            var addedHistories = toSnapshot.HistoryNames.Except(fromSnapshot.HistoryNames, StringComparer.InvariantCultureIgnoreCase);
            var deletedHistories = fromSnapshot.HistoryNames.Except(toSnapshot.HistoryNames, StringComparer.InvariantCultureIgnoreCase);
            var modifiedHistories = fromSnapshot.HistoryNames.Intersect(toSnapshot.HistoryNames, StringComparer.InvariantCultureIgnoreCase)
                .Where(name => !StringComparer.InvariantCultureIgnoreCase.Equals(toSnapshot.GetSnapshotId(name), fromSnapshot.GetSnapshotId(name)));

            var historyChanges = addedHistories.Select(name => new HistoryChange(name, ChangeType.Added))
                .Union(deletedHistories.Select(name => new HistoryChange(name, ChangeType.Deleted)))
                .Union(modifiedHistories.Select(name => new HistoryChange(name, ChangeType.Modified)));

            return new MultiFileSystemDiff(fromSnapshot, toSnapshot, fileChanges, historyChanges);
        }


        IMultiFileSystemSnapshot GetSnapshot(string id)
        {            
            var commit = Repository.Value.Lookup<Commit>(id);
            if (commit == null || !GitBasedMultiFileSystemSnapshot.IsSnapshot(commit))
            {
                throw new SnapshotNotFoundException(id);
            }
            else
            {
                return new GitBasedMultiFileSystemSnapshot(commit, m_HistoryService);
            }
        }
        
        IEnumerable<IMultiFileSystemChangeList> CombineChangeLists(string[] allHistoryNames, IEnumerable<Tuple<string, IFileSystemDiff>> diffs)
        {            
            // changes need to be combned per path
            var results = new Dictionary<string, MultiFileSystemChangeList>(StringComparer.InvariantCultureIgnoreCase);

            // every tuple is a single history name and all the changes for all the files from that history
            foreach (var tuple in diffs)
            {
                // iterate over all change lists and put it in the correct MultiFileSystemChangeList
                foreach (var changeList in tuple.Item2.ChangeLists)
                {
                    // first time we encouter that file path => create new change list
                    if (!results.ContainsKey(changeList.Path))
                    {
                        results.Add(changeList.Path, new MultiFileSystemChangeList(changeList.Path, allHistoryNames));
                    }                        

                    // add the changes from this history for this file to the combnedchange list
                    results[changeList.Path].SetChanges(tuple.Item1, changeList.Changes);                    
                }
            }

            return results.Values;            
        }

        void AssertIsAncestor(string ancestorId, string descandantId)
        {
            if (!Repository.Value.IsCommitAncestor(ancestorId, descandantId))
            {
                throw new InvalidRangeException($"Snapshot {descandantId} is not an descendant of {ancestorId}");
            }
        }
    }
}