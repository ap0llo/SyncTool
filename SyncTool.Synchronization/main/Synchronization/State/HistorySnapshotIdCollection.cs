// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncTool.Synchronization.State
{
    public sealed class HistorySnapshotIdCollection
    {
        readonly IDictionary<string, HistorySnapshotId> m_SnapshotIds;

        public IEnumerable<HistorySnapshotId> SnapshotIds => m_SnapshotIds.Values;


        public HistorySnapshotIdCollection(IEnumerable<HistorySnapshotId> snapshotIds)
        {
            if (snapshotIds == null)
                throw new ArgumentNullException(nameof(snapshotIds));

            m_SnapshotIds = snapshotIds.ToDictionary(id => id.HistoryName, StringComparer.InvariantCultureIgnoreCase);
        }


        public string GetSnapshotId(string historyName) => m_SnapshotIds[historyName].SnapshotId;
        

    }
}
