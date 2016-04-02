// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using SyncTool.Common;

namespace SyncTool.Synchronization.State
{
    public class SynchronizationStateNotFoundException : ItemNotFoundException
    {

        public int SynchronizationStateId { get; }


        public SynchronizationStateNotFoundException(int id) : base(id.ToString())
        {
            SynchronizationStateId = id;
        }
    }
}