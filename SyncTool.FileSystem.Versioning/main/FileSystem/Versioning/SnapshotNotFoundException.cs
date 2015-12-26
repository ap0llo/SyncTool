// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;

namespace SyncTool.FileSystem.Versioning
{
    [Serializable]
    public class SnapshotNotFoundException : Exception
    {

        public SnapshotNotFoundException(string id) : base($"Snapshot with id '{id}' not found")
        {
            
        }

    }
}