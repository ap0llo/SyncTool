// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using SyncTool.FileSystem;
using SyncTool.FileSystem.Versioning;
using SyncTool.Synchronization.ChangeGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncTool.Synchronization
{
    class ChangeGraphBuilder
    {
        readonly IEqualityComparer<IFileReference> m_FileReferenceComparer;


        public ChangeGraphBuilder(IEqualityComparer<IFileReference> fileReferenceComparer)
        {
            if(fileReferenceComparer == null)
            {
                throw new ArgumentNullException(nameof(fileReferenceComparer));
            }

            m_FileReferenceComparer = fileReferenceComparer;
        }


        public IEnumerable<Graph<IFileReference>> GetChangeGraphs(IEnumerable<IFileSystemDiff> diffs)
        {            
            diffs = diffs.ToList();

            var changeListsByFile = diffs
                .SelectMany(diff => diff.ChangeLists.Select(cl => new ChangeListWithHistoryName(diff.History.Name, cl)))
                .GroupBy(x => x.Path);

            foreach (var changeLists in changeListsByFile)
            {
                var path = changeLists.First().Path;

                var graph = new Graph<IFileReference>(m_FileReferenceComparer);

                // add ToVersion and FromVersion for every change to the graph
                var changes = changeLists.SelectMany(cl => cl.Changes).ToArray();
                graph.AddNodes(changes.Select(c => c.FromVersion));
                graph.AddNodes(changes.Select(c => c.ToVersion));

                // for each diff which has no changes, add the current file as node
                var historiesWithoutChanges = diffs
                    .Select(d => d.History.Name)
                    .Except(changeLists.Select(cl => cl.HistoryName), StringComparer.InvariantCultureIgnoreCase);

                foreach (var historyName in historiesWithoutChanges)
                {
                    var rootDirectory = diffs.Single(d => d.History.Name.Equals(historyName)).ToSnapshot.RootDirectory;
                    if (rootDirectory.FileExists(path))
                    {
                        graph.AddNodes(rootDirectory.GetFile(path).ToReference());
                    }
                    else
                    {
                        graph.AddNodes((IFileReference)null);
                    }
                }

                // add all edges to the graph
                foreach (var change in changes)
                {
                    graph.AddEdge(change.FromVersion, change.ToVersion);
                }

                yield return graph;
            }
            
        }



    }
}
