﻿// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;

namespace SyncTool.Synchronization
{
    [Serializable]
    public class NotApplicableException : Exception
    {
        public NotApplicableException(string message) : base(message)
        {           
        }
    }
}