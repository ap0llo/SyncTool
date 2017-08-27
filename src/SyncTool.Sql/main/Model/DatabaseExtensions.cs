using Dapper;
using System.Collections.Generic;
using System.Data;

namespace SyncTool.Sql.Model
{
    static class DatabaseExtensions
    {    
        public static int ExecuteNonQuery(this IDatabase database, string sql, params (string name, object value)[] parameters)
        {
            using (var connection = database.OpenConnection())
            {
                return connection.ExecuteNonQuery(sql, parameters);
            }                
        }

        public static T ExecuteScalar<T>(this IDatabase database, string sql, params (string name, object value)[] parameters)
        {
            using (var connection = database.OpenConnection())
            {
                return connection.ExecuteScalar<T>(sql, parameters);
            }            
        }

        public static IEnumerable<T> Query<T>(this IDatabase database, string sql, object param = null)
        {
            using (var connection = database.OpenConnection())
            {
                return connection.Query<T>(sql, param);
            }
        }


        public static T QuerySingleOrDefault<T>(this IDatabase database, string sql, object param = null)
        {
            using (var connection = database.OpenConnection())
            {
                return connection.QuerySingleOrDefault<T>(sql, param);
            }
        }

        public static T QuerySingle<T>(this IDatabase database, string sql, object param = null)
        {
            using (var connection = database.OpenConnection())
            {
                return connection.QuerySingle<T>(sql, param);
            }
        }

        public static T QueryFirstOrDefault<T>(this IDatabase database, string sql, object param = null)
        {
            using (var connection = database.OpenConnection())
            {
                return connection.QueryFirstOrDefault<T>(sql, param);
            }
        }
    }
}
