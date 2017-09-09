using System;

namespace SyncTool.Sql._Exceptions
{
    public class DatabaseNameMissingException : ArgumentException
    {
        public DatabaseNameMissingException()
        {
        }

        public DatabaseNameMissingException(string message) : base(message)
        {
        }
    }
}