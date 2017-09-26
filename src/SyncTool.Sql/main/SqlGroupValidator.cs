using System;
using SyncTool.Common.Groups;
using SyncTool.Sql.Model;
using Microsoft.Extensions.Logging;

namespace SyncTool.Sql
{
    public class SqlGroupValidator : IGroupValidator
    {
        readonly ILogger<SqlGroupValidator> m_Logger;

        public SqlGroupValidator(ILogger<SqlGroupValidator> logger)
        {
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void EnsureGroupIsValid(string groupName, string address)
        {
            m_Logger.LogDebug($"Validating group '{groupName}'");
            // try to open a connection to the database
            // this will make sure the address is valid and the database has a compatible schmea
            try
            {
                var database = new MySqlDatabase(new Uri(address));
                database.CheckSchema();
            }
            catch (InvalidDatabaseUriException ex)
            {
                throw new GroupValidationException($"Address '{address}' is not valid", ex);
            }
            catch (IncompatibleSchmeaException ex)
            {
                throw new GroupValidationException("Databse at '{address}' has an incompatible schema", ex);
            }            
            catch(Exception ex)
            {
                m_Logger.LogError(ex, "Unhandled exception during group validation");
                throw;
            }
        }
    }
}