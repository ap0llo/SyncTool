using SyncTool.FileSystem;
using SyncTool.FileSystem.Versioning;
using SyncTool.Synchronization.ChangeGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using SyncTool.Common;

namespace SyncTool.Synchronization
{
    class ChangeGraphBuilder
    {
        readonly IEqualityComparer<IFileReference> m_FileReferenceComparer;
        

        public ChangeGraphBuilder(IEqualityComparer<IFileReference> fileReferenceComparer)
        {
            if(fileReferenceComparer == null)
                throw new ArgumentNullException(nameof(fileReferenceComparer));
            

            m_FileReferenceComparer = fileReferenceComparer;        
        }

        public IEnumerable<Graph<IFileReference>> GetChangeGraphs(IMultiFileSystemDiff diff)
        {
            foreach (var changeList in diff.FileChanges)
            {
                var graph = new Graph<IFileReference>(m_FileReferenceComparer);

                foreach (var historyName in changeList.HistoryNames)
                {
                    var changes = changeList.GetChanges(historyName).ToArray();
                    
                    // add changes for the current history to the graph
                    if (changes.Any())
                    {
                        graph.AddEdgeFromStartNode(changes.First().FromVersion);
                        foreach (var change in changes)
                        {
                            graph.AddEdge(change.FromVersion, change.ToVersion);
                        }
                    }
                    // for histories without a change to the current file, add the current file version as node
                    else
                    {                        
                        var rootDirectory = diff.ToSnapshot.GetSnapshot(historyName).RootDirectory;
                        graph.AddEdgeFromStartNode(rootDirectory.GetFileReferenceOrDefault(changeList.Path));
                    }
                }

                yield return graph;
            }            
        }
               
    }
}
