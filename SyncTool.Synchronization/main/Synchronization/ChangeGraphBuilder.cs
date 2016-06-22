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
using SyncTool.Common;

namespace SyncTool.Synchronization
{
    [Obsolete]
    class ChangeGraphBuilder
    {
        readonly IEqualityComparer<IFileReference> m_FileReferenceComparer;
        readonly IGroup m_Group;


        public ChangeGraphBuilder(IEqualityComparer<IFileReference> fileReferenceComparer, IGroup group)
        {
            if(fileReferenceComparer == null)
            {
                throw new ArgumentNullException(nameof(fileReferenceComparer));
            }
            if (group == null)
            {
                throw new ArgumentNullException(nameof(@group));
            }

            m_FileReferenceComparer = fileReferenceComparer;
            m_Group = @group;
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
