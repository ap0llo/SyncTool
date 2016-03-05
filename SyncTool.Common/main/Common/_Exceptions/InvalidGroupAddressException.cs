// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;

namespace SyncTool.Common
{
    public class InvalidGroupAddressException : GroupManagerException
    {
        public InvalidGroupAddressException(string message) : base(message)
        {
            
        }

        public InvalidGroupAddressException(string message, Exception innerException) : base(message, innerException)
        {

        }

        public static InvalidGroupAddressException FromAdress(string addresss, Exception innerException = null)
        {
            return new InvalidGroupAddressException($"'{addresss}' is not the adress of a valid group", innerException);
        }
    }
}