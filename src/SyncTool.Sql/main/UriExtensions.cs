using System;
using JetBrains.Annotations;
using MySql.Data.MySqlClient;

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

            if(uri.Scheme != s_Scheme)
                throw new ArgumentException($"Unsupported scheme '{uri.Scheme}'", nameof(uri));

            if (String.IsNullOrEmpty(uri.Host))
                throw new ArgumentException("Host must not be empty", nameof(uri));

            if(uri.Segments.Length > 2)
                throw new ArgumentException("Uri must not contain multiple segments", nameof(uri));

            var (user, password) = ParseUserInfo(uri.UserInfo);

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


        static (string user, string password) ParseUserInfo(string userInfo)
        {
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
                    throw new ArgumentException($"'{userInfo}' is not a valid user info");
            }
        }
    }
}