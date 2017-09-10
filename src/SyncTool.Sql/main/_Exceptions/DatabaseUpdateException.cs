using System;

namespace SyncTool.Sql
{
    /// <summary>
    /// Exception that is thrown when a item in the database could not be updated
    /// </summary>
    public class DatabaseUpdateException : DatabaseException
    {
        public DatabaseUpdateException()
        {
        }

        public DatabaseUpdateException(string message) : base(message)
        {
        }

        public DatabaseUpdateException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
