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
            if (diffs == null)
            {
                throw new ArgumentNullException(nameof(diffs));
            }

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
                var changeListsByFolderName = changeLists.ToDictionary(cl => cl.HistoryName , cl => cl.ChangeList, StringComparer.InvariantCultureIgnoreCase);
                yield return GetChangeGraph(diffsByFolderName, changeLists.Key, changeListsByFolderName);
            }

        }
        

        Graph<IFileReference> GetChangeGraph(IDictionary<string, IFileSystemDiff> diffsByFolderName, string path, IDictionary<string, IChangeList> changeListsByFolderName)
        {            
            var graph = new Graph<IFileReference>(m_FileReferenceComparer);                        

            // add ToVersion and FromVersion for every change to the graph
            var changes = changeListsByFolderName.Values.SelectMany(cl => cl.Changes).ToArray();
            graph.AddNodes(changes.Select(c => c.FromVersion));
            graph.AddNodes(changes.Select(c => c.ToVersion));
            
            // for each diff which has no changes, add the current file as node
            var foldersWithoutChanges = diffsByFolderName.Keys.Except(changeListsByFolderName.Keys, StringComparer.InvariantCultureIgnoreCase);
            foreach (var folderName in foldersWithoutChanges)
            {
                var rootDirectory = diffsByFolderName[folderName].ToSnapshot.RootDirectory;
                graph.AddNode(rootDirectory.GetFileReferenceOrDefault(path));
            }


            // add all edges to the graph
            foreach (var change in changes)
            {
                graph.AddEdge(change.FromVersion, change.ToVersion);
            }

            return graph;
        }
        
       
    }
}
