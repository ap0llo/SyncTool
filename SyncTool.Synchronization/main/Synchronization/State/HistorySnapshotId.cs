// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncTool.Synchronization.State
{
    public sealed class HistorySnapshotId : IEquatable<HistorySnapshotId>
    {
        public string HistoryName { get; }

        public string SnapshotId { get; }


        public HistorySnapshotId(string historyName, string snapshotId)
        {
            if (String.IsNullOrWhiteSpace(historyName))
                throw new ArgumentException("Value must not be empty", nameof(historyName));

            if(snapshotId != null && String.IsNullOrWhiteSpace(snapshotId))
                throw new ArgumentException("Value must not be empty", nameof(snapshotId));

            HistoryName = historyName;
            SnapshotId = snapshotId;
        }


        public override int GetHashCode() => StringComparer.InvariantCultureIgnoreCase.GetHashCode(HistoryName + SnapshotId);

        public override bool Equals(object obj) => Equals(obj as HistorySnapshotId);
 
        public bool Equals(HistorySnapshotId other)
        {
            if(other == null)
            {
                return false;
            }

            return StringComparer.InvariantCultureIgnoreCase.Equals(HistoryName, other.HistoryName) &&
                   StringComparer.InvariantCultureIgnoreCase.Equals(SnapshotId, other.SnapshotId);
        }
    }
}
