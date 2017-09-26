using System;
using SyncTool.Common.Groups;
using SyncTool.Sql.Model;
using Microsoft.Extensions.Logging;

namespace SyncTool.Sql
{
    public class SqlGroupInitializer : IGroupInitializer
    {
        readonly ILogger<SqlGroupInitializer> m_Logger;
        readonly Func<Uri, MySqlDatabase> m_DatabaseFactory;

        public SqlGroupInitializer(ILogger<SqlGroupInitializer> logger, Func<Uri, MySqlDatabase> databaseFacotry)
        {
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            m_DatabaseFactory = databaseFacotry ?? throw new ArgumentNullException(nameof(databaseFacotry));
        }

        public void Initialize(string groupName, string address)
        {
            m_Logger.LogDebug($"Initializing group '{groupName}'");
            try
            {
                var databaseUri = new Uri(address);
                var database = m_DatabaseFactory.Invoke(databaseUri);
                database.Create();
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