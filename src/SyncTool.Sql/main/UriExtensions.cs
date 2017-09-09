using System;
using JetBrains.Annotations;
using MySql.Data.MySqlClient;
using SyncTool.Sql;

namespace SyncTool.Sql
{
    public static class UriExtensions
    {
        const string s_Scheme = "synctool-mysql";


        public static string ToMySqlConnectionString([NotNull] this Uri uri) => uri.ToMySqlConnectionStringBuilder().ConnectionString;        
        
        public static MySqlConnectionStringBuilder ToMySqlConnectionStringBuilder([NotNull] this Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            if(!uri.IsSyncToolMySqlUri())
                throw new InvalidDatabaseUriException(uri, $"Unsupported scheme '{uri.Scheme}'");

            if (String.IsNullOrEmpty(uri.Host))
                throw new InvalidDatabaseUriException(uri, "Host must not be empty");

            if(uri.Segments.Length > 2)
                throw new InvalidDatabaseUriException(uri, "Uri must not contain multiple segments");

            var (user, password) = GetUserInfo(uri);

            var connectionStringBuilder = new MySqlConnectionStringBuilder()            
            {
                Server = uri.Host,     
                Database = uri.Segments.Length == 2 ? uri.Segments[1] : null,
                UserID = user,
                Password = password
            };

            if (uri.Port > 0)
                connectionStringBuilder.Port = (uint) uri.Port;

            return connectionStringBuilder;
        }

        public static bool IsSyncToolMySqlUri(this Uri uri) => uri.Scheme == s_Scheme;


        static (string user, string password) GetUserInfo(Uri uri)
        {
            var userInfo = uri.UserInfo;

            if (String.IsNullOrEmpty(userInfo))
                return (null, null);
        
            var fragments = userInfo.Split(new[] {':'}, StringSplitOptions.RemoveEmptyEntries);

            switch (fragments.Length)
            {
                case 0:
                    // should not happen (userInfo was already checked for null or empty)
                    throw new InvalidOperationException();
                case 1:
                    return (fragments[0], null);
                case 2:
                    return (fragments[0], fragments[1]);
                default:
                    throw new InvalidDatabaseUriException(uri, "UserInfo is not a valid");
            }
        }
    }
}