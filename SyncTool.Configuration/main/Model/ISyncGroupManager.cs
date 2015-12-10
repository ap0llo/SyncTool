// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace SyncTool.Configuration.Model
{
    public interface ISyncGroupManager
    {
        IEnumerable<string> SyncGroups { get; }

        ISyncGroup GetSyncGroup(string name);

        void AddSyncGroup(string name);

        void RemoveSyncGroup(string name);
    }
}