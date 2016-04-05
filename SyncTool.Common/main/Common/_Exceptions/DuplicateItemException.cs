// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Runtime.Serialization;

namespace SyncTool.Common
{
    [Serializable]
    public class DuplicateItemException : Exception
    {
        public DuplicateItemException(string message) : base(message)
        {
        }

        public DuplicateItemException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DuplicateItemException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

    }
}