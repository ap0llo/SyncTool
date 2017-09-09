using System;

namespace SyncTool.Sql
{
    public class InvalidDatabaseUriException : ArgumentException
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