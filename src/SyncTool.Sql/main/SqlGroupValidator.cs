using System;
using SyncTool.Common.Groups;
using SyncTool.Sql.Model;

namespace SyncTool.Sql
{
    public class SqlGroupValidator : IGroupValidator
    {
        public void EnsureGroupIsValid(string groupName, string address)
        {
            // try to open a connection to the database
            // this will make sure the address is valid and the database has a compatible schmea
            try
            {
                var database = new MySqlDatabase(new Uri(address));
                database.CheckSchema();
            }
            catch (InvalidDatabaseUriException ex)
            {
                throw new GroupInitializationException($"Address '{address}' is not valid", ex);
            }
            catch (IncompatibleSchmeaException ex)
            {
                throw new GroupValidationException("Databse at '{address}' has an incompatible schema", ex);
            }            
        }
    }
}