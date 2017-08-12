using System;

namespace SyncTool.Sql.Model
{
    public class DatabaseUpdateException : Exception
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
