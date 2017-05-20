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