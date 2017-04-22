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