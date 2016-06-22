// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using SyncTool.Common;
using SyncTool.FileSystem;
using SyncTool.Synchronization.State;

namespace SyncTool.Synchronization.ChangeGraph
{
    public interface IChangeGraphService : IService
    {
        IEnumerable<Graph<IFileReference>> GetChangeGraphs(HistorySnapshotIdCollection to, string[] pathFilter = null);

        IEnumerable<Graph<IFileReference>> GetChangeGraphs(HistorySnapshotIdCollection from, HistorySnapshotIdCollection to, string[] pathFilter = null);        
    }
}