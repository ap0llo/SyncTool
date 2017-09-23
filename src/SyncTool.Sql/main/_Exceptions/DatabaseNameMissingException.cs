using System;

namespace SyncTool.Sql
{
    public class DatabaseNameMissingException : InvalidDatabaseUriException
    {
        public DatabaseNameMissingException(Uri databaseUri) 
            : base(databaseUri, $"Database uri '{databaseUri}' does not specify a database name")
        {            
        }
    }
}