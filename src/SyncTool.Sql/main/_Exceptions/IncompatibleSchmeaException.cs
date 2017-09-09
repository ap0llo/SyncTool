namespace SyncTool.Sql
{
    public class IncompatibleSchmeaException : DatabaseException
    {
        /// <summary>
        /// The schema version supported by the current application version
        /// </summary>
        public int SupportedSchemaVersion { get; }

        /// <summary>
        /// The schema version present in the database
        /// </summary>
        public int DatabaseSchemaVersion { get; }


        public IncompatibleSchmeaException(int supportedVersion, int databaseVersion)
            : base($"The version of the schema in the database ({databaseVersion}) is not supported by " +
                   $"this application version, which only supports schema version {supportedVersion}")
        {
            SupportedSchemaVersion = supportedVersion;
            DatabaseSchemaVersion = databaseVersion;
        }        
    }
}