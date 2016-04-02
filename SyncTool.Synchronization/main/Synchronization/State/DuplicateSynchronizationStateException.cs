// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using SyncTool.FileSystem.Versioning;

namespace SyncTool.Synchronization.State
{
    public class DuplicateSynchronizationStateException : Exception
    {

        public DuplicateSynchronizationStateException(int id) : base($"A synchronization state with id {id} already exists")
        {
            
        }

    }
}