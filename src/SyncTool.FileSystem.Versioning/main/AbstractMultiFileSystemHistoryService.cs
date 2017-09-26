using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace SyncTool.FileSystem.Versioning
{
    public abstract class AbstractMultiFileSystemHistoryService : IMultiFileSystemHistoryService
    {
        readonly ILogger<AbstractMultiFileSystemHistoryService> m_Logger;
        readonly IHistoryService m_HistoryService;


        public abstract IMultiFileSystemSnapshot LatestSnapshot { get; }

        public abstract IEnumerable<IMultiFileSystemSnapshot> Snapshots { get; }

        public IMultiFileSystemSnapshot this[string id] => String.IsNullOrWhiteSpace(id) ? throw new ArgumentNullException(nameof(id)) : GetSnapshot(id);


        protected AbstractMultiFileSystemHistoryService(
            [NotNull] ILogger<AbstractMultiFileSystemHistoryService> logger,
            [NotNull] IHistoryService historyService)
        {
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            m_HistoryService = historyService ?? throw new ArgumentNullException(nameof(historyService));
        }


        public abstract IMultiFileSystemSnapshot CreateSnapshot();

        public string[] GetChangedFiles(string toId)
        {
            m_Logger.LogDebug($"Getting changed files up to snapshot {toId}");

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
            m_Logger.LogDebug($"Getting changed files between snapshot {from} and {to}");

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

                string[] changedFiles = Array.Empty<string>();
                // if the history already existed at the time of fromSnapshot,                 
                // only get the changes between the snapshots                
                if (fromSnapshot.HistoryNames.Contains(histoyName))
                {
                    var fromId = fromSnapshot.GetSnapshotId(histoyName);
                    var toId = toSnapshot.GetSnapshotId(histoyName);

                    // fromId == toId 
                    //  => no snapshot was added for this history
                    //  => no changes to add
                    if (fromId != toId)
                    {
                        changedFiles = history.GetChangedFiles(fromId, toId);
                    }                    
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
            m_Logger.LogDebug($"Getting changes up to snapshot {toId}");

            if (String.IsNullOrWhiteSpace(toId))
                throw new ArgumentNullException(nameof(toId));

            var snapshot = GetSnapshot(toId);

            // get all change lists from all histories
            var diffs = snapshot.HistoryNames
                .Select(name => m_HistoryService[name].GetChanges(snapshot.GetSnapshotId(name)))
                .Select(diff => (diff.History.Name, diff));

            var fileChanges = CombineChangeLists(snapshot.HistoryNames.ToArray(), diffs);

            // since we're getting all changes up to the specified snapshot,
            // all histories were added (initially there were none)
            var historyChanges = snapshot.HistoryNames.Select(name => new HistoryChange(name, ChangeType.Added));

            return new MultiFileSystemDiff(snapshot, fileChanges, historyChanges);
        }

        public IMultiFileSystemDiff GetChanges(string fromId, string toId, string[] pathFilter = null)
        {
            m_Logger.LogDebug($"Getting changes up between snapshot {fromId} and {toId}");

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

                // if the history already existed at the time of fromSnapshot, 
                // only get the changes between the snapshots                
                if (fromSnapshot.HistoryNames.Contains(histoyName))
                {
                    var currentHistoryFromId = fromSnapshot.GetSnapshotId(histoyName);
                    var currentHistoryToId = toSnapshot.GetSnapshotId(histoyName);

                    // fromId == toId 
                    //  => no snapshot was added for this history
                    //  => no changes to add
                    if (currentHistoryFromId != currentHistoryToId)
                    {
                        diffs.Add(history.GetChanges(currentHistoryFromId, currentHistoryToId));
                    }
                }
                // if the current history did not exist at the time of fromSnapshot, 
                // get all changes for the current history
                else
                {
                    diffs.Add(history.GetChanges(toSnapshot.GetSnapshotId(histoyName)));
                }
            }

            var fileChanges = CombineChangeLists(
                toSnapshot.HistoryNames.ToArray(),
                diffs.Select(diff => (diff.History.Name, diff))
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

        
        protected abstract IMultiFileSystemSnapshot GetSnapshot(string id);

        /// <summary>
        /// Checks if the snapshot with id <paramref name="ancestorId"/> is an ancestor of
        /// <paramref name="descandantId"/>.
        /// </summary>
        /// <remarks>
        /// Implementations can assume both snapshots exist
        /// </remarks>
        /// <exception cref="InvalidRangeException">Thrown if the descandant snapshot is not a descandent of the specified ancestor</exception>
        protected abstract void AssertIsAncestor(string ancestorId, string descandantId);


        IEnumerable<IMultiFileSystemChangeList> CombineChangeLists(string[] allHistoryNames, IEnumerable<(string historyName, IFileSystemDiff diff)> historyDiffs)
        {
            // changes need to be combned per path
            var results = new Dictionary<string, MultiFileSystemChangeList>(StringComparer.InvariantCultureIgnoreCase);

            // every tuple is a single history name and all the changes for all the files from that history
            foreach (var tuple in historyDiffs)
            {
                // iterate over all change lists and put it in the correct MultiFileSystemChangeList
                foreach (var changeList in tuple.diff.ChangeLists)
                {
                    // first time we encouter that file path => create new change list
                    if (!results.ContainsKey(changeList.Path))
                    {
                        results.Add(changeList.Path, new MultiFileSystemChangeList(changeList.Path, allHistoryNames));
                    }

                    // add the changes from this history for this file to the combnedchange list
                    results[changeList.Path].SetChanges(tuple.historyName, changeList.Changes);
                }
            }

            return results.Values;
        }
    }
}