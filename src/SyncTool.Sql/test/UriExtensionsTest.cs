using System;
using Xunit;

namespace SyncTool.Sql.Test
{
    public class UriExtensionsTest
    {
        [Theory]
        [InlineData("synctool-mysql://user:password@host:123/databasename", "host", 123, "databasename", "user", "password")]
        [InlineData("syncTOOL-MYSQL://user:password@host:123/databasename", "host", 123, "databasename", "user", "password")]
        [InlineData("synctool-mysql://user:password@host/databasename", "host", 3306, "databasename", "user", "password")]
        [InlineData("synctool-mysql://user:password@host:123", "host", 123, "", "user", "password")]
        [InlineData("synctool-mysql://user:password@host:123/", "host", 123, "", "user", "password")]
        [InlineData("synctool-mysql://user@host:123/databasename", "host", 123, "databasename", "user", "")]
        [InlineData("synctool-mysql://host:123/databasename", "host", 123, "databasename", "", "")]
        public void ToMySqlConnectionStringBuilder_returns_expected_values(string uri, string expectedServer, uint expectedPort, string expectedDatabaseName, string expectedUserId, string expectedPassword)
        {
            var connectionStringBuilder = new Uri(uri).ToMySqlConnectionStringBuilder();

            Assert.Equal(expectedServer, connectionStringBuilder.Server);
            Assert.Equal(expectedPort, connectionStringBuilder.Port);
            Assert.Equal(expectedDatabaseName, connectionStringBuilder.Database);
            Assert.Equal(expectedUserId, connectionStringBuilder.UserID);
            Assert.Equal(expectedPassword, connectionStringBuilder.Password);
        }

        [Theory]
        [InlineData("http://www.example.com")]
        [InlineData("https://www.example.com")]
        [InlineData("file://www.example.com")]
        public void ToMySqlConnectionStringBuilder_throws_ArgumentException_if_scheme_if_unknown(string uri)
        {
            Assert.Throws<ArgumentException>(() => new Uri(uri).ToMySqlConnectionStringBuilder());
        }

        [Theory]
        [InlineData("synctool-mysql://host:123/databasename/anothername")]
        public void ToMySqlConnectionStringBuilder_throws_ArgumentException_if_path_contains_multiple_segments(string uri)
        {
            Assert.Throws<ArgumentException>(() => new Uri(uri).ToMySqlConnectionStringBuilder());
        }

        [Theory]
        [InlineData("synctool-mysql:///databasename")]
        public void ToMySqlConnectionStringBuilder_throws_ArgumentException_if_host_is_null(string uri)
        {
            Assert.Throws<ArgumentException>(() => new Uri(uri).ToMySqlConnectionStringBuilder());
        }

        [Theory]
        [InlineData("synctool-mysql://user:password:more@host/databasename")]
        public void ToMySqlConnectionStringBuilder_throws_ArgumentException_UserInfo_is_in_invalid_format(string uri)
        {
            Assert.Throws<ArgumentException>(() => new Uri(uri).ToMySqlConnectionStringBuilder());
        }
    }
}