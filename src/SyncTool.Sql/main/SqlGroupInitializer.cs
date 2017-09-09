using System;
using SyncTool.Common.Groups;
using SyncTool.Sql.Model;

namespace SyncTool.Sql
{
    public class SqlGroupInitializer : IGroupInitializer
    {
        public void Initialize(string groupName, string address)
        {
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
                throw new GroupInitializationException("Cannot initialize group becuase the specified database has an incompatible schema", ex);
            }
            catch (DatabaseException ex)
            {
                throw new GroupInitializationException($"Error initializing group '{groupName}'", ex);
            }
        }
    }
}