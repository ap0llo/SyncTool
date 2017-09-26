using System;
using SyncTool.Common.Groups;
using SyncTool.Sql.Model;
using Microsoft.Extensions.Logging;

namespace SyncTool.Sql
{
    public class SqlGroupInitializer : IGroupInitializer
    {
        readonly ILogger<SqlGroupInitializer> m_Logger;

        public SqlGroupInitializer(ILogger<SqlGroupInitializer> logger)
        {
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Initialize(string groupName, string address)
        {
            m_Logger.LogDebug($"Initializing group '{groupName}'");
            try
            {
                var databaseUri = new Uri(address);
                MySqlDatabase.Create(databaseUri);
            }
            catch (InvalidDatabaseUriException ex)
            {
                throw new GroupInitializationException($"Error initializing group '{groupName}'. Address '{address}' is not valid", ex);
            }
            catch (IncompatibleSchmeaException ex)
            {
                throw new GroupInitializationException("Cannot initialize group because the specified database has an incompatible schema", ex);
            }
            catch (DatabaseException ex)
            {
                throw new GroupInitializationException($"Error initializing group '{groupName}'", ex);
            }
            catch (Exception ex)
            {
                m_Logger.LogError(ex, "Unhandled exception during group initialization");
                throw;
            }
        }
    }
}