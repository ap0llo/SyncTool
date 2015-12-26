// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;

namespace SyncTool.Common
{
    public class GroupManagerException : Exception
    {
        public GroupManagerException(string message, Exception innerException) : base(message, innerException)
        {

        }


        public GroupManagerException(string message) : base(message)
        {

        }
    }
}