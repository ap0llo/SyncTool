// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncTool.Synchronization.State
{
    //TODO: There needs to be a better name for this class
    public sealed class HistorySnapshotIdCollection : IEnumerable<HistorySnapshotId>
    {        
        public static readonly HistorySnapshotIdCollection Empty = new HistorySnapshotIdCollection(Enumerable.Empty<HistorySnapshotId>());


        readonly IDictionary<string, HistorySnapshotId> m_SnapshotIds;

        public IEnumerable<string> HistoryNames => m_SnapshotIds.Keys; 

        public IEnumerable<HistorySnapshotId> SnapshotIds => m_SnapshotIds.Values;

        public HistorySnapshotIdCollection(params HistorySnapshotId[] snapshotIds) : this((IEnumerable<HistorySnapshotId>)snapshotIds)
        {

        }

        public HistorySnapshotIdCollection(IEnumerable<HistorySnapshotId> snapshotIds)
        {
            if (snapshotIds == null)
                throw new ArgumentNullException(nameof(snapshotIds));

            m_SnapshotIds = snapshotIds.ToDictionary(id => id.HistoryName, StringComparer.InvariantCultureIgnoreCase);
        }


        public string GetSnapshotId(string historyName) => m_SnapshotIds[historyName].SnapshotId;

        public bool ContainsHistoryName(string name) => m_SnapshotIds.ContainsKey(name);

        public IEnumerator<HistorySnapshotId> GetEnumerator() => m_SnapshotIds.Values.GetEnumerator();
        
        IEnumerator IEnumerable.GetEnumerator() => m_SnapshotIds.Values.GetEnumerator();

    }
}
