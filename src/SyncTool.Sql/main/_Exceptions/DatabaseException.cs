using System;

namespace SyncTool.Sql
{
    /// <summary>
    /// Generic exception class for unhandled errors that occurred when acessing a SQL database
    /// </summary>
    public class DatabaseException : Exception
    {
        public DatabaseException()
        {
        }

        public DatabaseException(string message) : base(message)
        {
        }

        public DatabaseException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
