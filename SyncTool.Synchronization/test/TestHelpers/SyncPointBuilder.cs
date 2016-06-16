// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using SyncTool.Configuration.Model;
using SyncTool.Synchronization.State;
using System.Linq;

namespace SyncTool.TestHelpers
{
    public static class SyncPointBuilder
    {

        public static MutableSyncPoint NewSyncPoint()
        {
            return new MutableSyncPoint();
        }


        public static MutableSyncPoint WithId(this MutableSyncPoint state, int id)
        {
            state.Id = id;
            return state;
        }



        public static MutableSyncPoint WithoutFromSnapshots(this MutableSyncPoint state)
        {
            state.FromSnapshots = HistorySnapshotIdCollection.Empty;            
            return state;
        }

        public static MutableSyncPoint WithToSnapshot(this MutableSyncPoint state, string name, string id)
        {
            HistorySnapshotIdCollection current;
            try
            {
                current = state.ToSnapshots;
            }
            catch (Exception)
            {
                current = null;
            }
            current = current ?? HistorySnapshotIdCollection.Empty;
            
            current = new HistorySnapshotIdCollection(current.Union(new[] { new HistorySnapshotId(name, id) }));
            state.ToSnapshots = current;
            
            return state;
        }

        public static MutableSyncPoint WithFromSnapshot(this MutableSyncPoint state, string name, string id)
        {
            HistorySnapshotIdCollection current;
            try
            {
                current = state.FromSnapshots;
            }
            catch (Exception)
            {
                current = null;
            }
            current = current ?? HistorySnapshotIdCollection.Empty;

            current = new HistorySnapshotIdCollection(current.Union(new[] { new HistorySnapshotId(name, id) }));
            state.FromSnapshots = current;

            return state;
        }


        public static MutableSyncPoint WithFilterConfiguration(this MutableSyncPoint state, string name, FilterConfiguration filterConfiguration)
        {
            Dictionary<string, FilterConfiguration> current;
            try
            {
                current = (Dictionary<string, FilterConfiguration>)state.FilterConfigurations;
            }
            catch (Exception)
            {
                current = null;
            }
            current = current ?? new Dictionary<string, FilterConfiguration>();

            current.Add(name, filterConfiguration);

            state.FilterConfigurations = current;

            return state;
        }

    }
}
