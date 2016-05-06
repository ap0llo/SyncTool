// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Runtime.Serialization;
using SyncTool.Common;

namespace SyncTool.Configuration
{
    public class SyncFolderNotFoundException : ItemNotFoundException
    {
        public SyncFolderNotFoundException(string message) : base(message)
        {
        }

        public SyncFolderNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public SyncFolderNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}