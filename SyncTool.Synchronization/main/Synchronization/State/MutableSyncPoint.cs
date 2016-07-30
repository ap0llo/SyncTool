// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using SyncTool.Configuration.Model;

namespace SyncTool.Synchronization.State
{
    public class MutableSyncPoint : ISyncPoint
    {
        public int Id { get; set; }
        
        public string MultiFileSystemSnapshotId { get; set; }

        public IReadOnlyDictionary<string, FilterConfiguration> FilterConfigurations { get; set; }
    }
}