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
    public class DuplicateSyncPointException : DuplicateItemException
    {

        public DuplicateSyncPointException(int id) : base($"A synchronization state with id {id} already exists")
        {
            
        }

        protected DuplicateSyncPointException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}