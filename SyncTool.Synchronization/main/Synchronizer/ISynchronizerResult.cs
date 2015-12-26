// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using SyncTool.FileSystem;

namespace SyncTool.Synchronization
{
    public interface ISynchronizerResult 
    {
        IEnumerable<SyncAction> Actions { get; }

        IEnumerable<SyncConflict> Conflicts { get; }

        IDirectory ApplyTo(IDirectory directory);
    }
}