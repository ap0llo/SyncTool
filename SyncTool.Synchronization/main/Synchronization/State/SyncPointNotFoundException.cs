// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Runtime.Serialization;
using SyncTool.Common;

namespace SyncTool.Synchronization.State
{
    [Serializable]
    public class SyncPointNotFoundException : ItemNotFoundException
    {

        public int SynchronizationStateId { get; }


        public SyncPointNotFoundException(int id) : base($"A SynchronizationState with id {id} could not be found")
        {
            SynchronizationStateId = id;
        }

        protected SyncPointNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}