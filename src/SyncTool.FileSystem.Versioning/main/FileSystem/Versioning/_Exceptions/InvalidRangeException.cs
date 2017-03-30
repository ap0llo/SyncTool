// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Runtime.Serialization;

namespace SyncTool.FileSystem.Versioning
{
    public class InvalidRangeException : Exception
    {
        public InvalidRangeException()
        {
        }

        public InvalidRangeException(string message) : base(message)
        {
        }

        public InvalidRangeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidRangeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}