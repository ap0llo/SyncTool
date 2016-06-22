// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using SyncTool.Common;
using SyncTool.FileSystem;
using SyncTool.FileSystem.Versioning;
using SyncTool.Synchronization.State;

namespace SyncTool.Synchronization.ChangeGraph
{
    public class ChangeGraphService : IChangeGraphService
    {
        readonly IEqualityComparer<IFileReference> m_FileReferenceComparer;
        readonly IHistoryService m_HistoryService;


        public ChangeGraphService(IEqualityComparer<IFileReference> fileReferenceComparer, IHistoryService historyService)
        {
            if (fileReferenceComparer == null)
                throw new ArgumentNullException(nameof(fileReferenceComparer));

            if (historyService == null)
                throw new ArgumentNullException(nameof(historyService));

            m_FileReferenceComparer = fileReferenceComparer;
            m_HistoryService = historyService;
        }


        public IEnumerable<Graph<IFileReference>> GetChangeGraphs(HistorySnapshotIdCollection to, string[] pathFilter = null)
        {
            if (to == null)
                throw new ArgumentNullException(nameof(to));

            if (!to.Any())
                throw new ArgumentException($"{nameof(HistorySnapshotIdCollection)} must not be empty", nameof(to));

            
            var diffs = to.Select(id => m_HistoryService[id.HistoryName].GetChanges(id.SnapshotId, pathFilter));

            return GetChangeGraphs(diffs);
        }

        public IEnumerable<Graph<IFileReference>> GetChangeGraphs(HistorySnapshotIdCollection from, HistorySnapshotIdCollection to, string[] pathFilter = null)
        {
            if (from == null)            
                throw new ArgumentNullException(nameof(from));
            
            if (to == null)            
                throw new ArgumentNullException(nameof(to));

            if(!from.Any())
                throw new ArgumentException($"{nameof(HistorySnapshotIdCollection)} must not be empty", nameof(from));

            if (!to.Any())
                throw new ArgumentException($"{nameof(HistorySnapshotIdCollection)} must not be empty", nameof(to));

            if (to.HistoryNames.Except(from.HistoryNames, StringComparer.InvariantCultureIgnoreCase).Any() ||
                from.HistoryNames.Except(to.HistoryNames, StringComparer.InvariantCultureIgnoreCase).Any())
            {
                throw new ArgumentException($"Both {nameof(from)} and {nameof(to)} must refer to the same set of histories");                
            }

            var diffs = from.Select(id => m_HistoryService[id.HistoryName].GetChanges(
                    id.SnapshotId, 
                    to.GetSnapshotId(id.HistoryName), 
                    pathFilter));

            return GetChangeGraphs(diffs);
        }


        IEnumerable<Graph<IFileReference>> GetChangeGraphs(IEnumerable<IFileSystemDiff> diffs)
        {
            var diffsByFolderName = diffs.ToDictionary(d => d.History.Name, StringComparer.InvariantCultureIgnoreCase);

            // group the changes by path (there will be one graph per file)
            var changeListsByFile = diffsByFolderName.Values
                .SelectMany(diff =>
                    diff.ChangeLists.Select(cl => new
                    {
                        HistoryName = diff.History.Name,
                        ChangeList = cl
                    }))
                .GroupBy(x => x.ChangeList.Path);


            foreach (var changeLists in changeListsByFile)
            {
                var changeListsByFolderName = changeLists.ToDictionary(cl => cl.HistoryName, cl => cl.ChangeList, StringComparer.InvariantCultureIgnoreCase);
                yield return GetChangeGraph(diffsByFolderName, changeLists.Key, changeListsByFolderName);
            }
        }
            
        Graph<IFileReference> GetChangeGraph(IDictionary<string, IFileSystemDiff> diffsByFolderName, string path, IDictionary<string, IChangeList> changeListsByFolderName)
        {
            var graph = new Graph<IFileReference>(m_FileReferenceComparer);

            foreach (var folderName in diffsByFolderName.Keys)
            {
                // add ToVersion and FromVersion for every change to the graph
                if (changeListsByFolderName.ContainsKey(folderName))
                {
                    var changeList = changeListsByFolderName[folderName];

                    graph.AddEdgeFromStartNode(changeList.Changes.First().FromVersion);

                    foreach (var change in changeList.Changes)
                    {
                        graph.AddEdge(change.FromVersion, change.ToVersion);
                    }

                }
                // for each diff which has no changes, add the current file as node
                else
                {
                    var rootDirectory = diffsByFolderName[folderName].ToSnapshot.RootDirectory;
                    graph.AddEdgeFromStartNode(rootDirectory.GetFileReferenceOrDefault(path));
                }
            }

            return graph;
        }

    }
}