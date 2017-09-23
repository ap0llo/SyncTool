using System;

namespace SyncTool.Sql
{
    public class InvalidDatabaseUriException : DatabaseException
    {
        public Uri DatabaseUri { get; }


        public InvalidDatabaseUriException(Uri databaseUri)
        {
            DatabaseUri = databaseUri;
        }

        public InvalidDatabaseUriException(Uri databaseUri, string message) : base(message)
        {
            DatabaseUri = databaseUri;
        }
    }
}