using System;
using System.Data;
using System.Data.Common;

namespace SyncTool.Sql.Model
{
    public static class DbConnectionExtensions
    {
        public static int ExecuteNonQuery(this IDbConnection connection, string sql, params (string name, object value)[] parameters)
        {
            var command = connection.CreateCommand(sql, parameters);            
            return command.ExecuteNonQuery();
        }
        
        public static T ExecuteScalar<T>(this IDbConnection connection, string sql, params (string name, object value)[] parameters)
        {
            var command = connection.CreateCommand(sql, parameters);
            return (T)Convert.ChangeType(command.ExecuteScalar(), typeof(T));
        }

        public static bool TableExists(this IDbConnection connection, string tableName)
        {
            try
            {
                return connection.ExecuteScalar<int>($"SELECT 1 FROM {tableName}") == 1;
            }
            catch (DbException)
            {
                return false;
            } 
        }

        static IDbCommand CreateCommand(this IDbConnection connection, string sql, params (string name, object value)[] parameters)
        {
            var command = connection.CreateCommand();
            command.CommandText = sql;

            foreach (var (name, value) in parameters)
            {
                command.AddParameter(name, value);
            }

            return command;
        }

        static void AddParameter(this IDbCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }
    }
}
