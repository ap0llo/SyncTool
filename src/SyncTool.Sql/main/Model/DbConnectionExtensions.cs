using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
