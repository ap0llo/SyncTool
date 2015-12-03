// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace SyncTool.Configuration.Model
{
    public interface ISyncGroupManager
    {
        IEnumerable<ISyncGroup> SyncGroups { get; }

        ISyncGroup this[string name] { get; }

        ISyncGroup AddSyncGroup(string name);

        void RemoveSyncGroup(string name);
    }
}