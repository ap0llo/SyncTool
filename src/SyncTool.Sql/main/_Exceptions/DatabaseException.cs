using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncTool.Sql
{
    /// <summary>
    /// Generic exception class for unhandled errors that occurred when acessing a SQL database
    /// </summary>
    public class DatabaseException : Exception
    {
        public DatabaseException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
