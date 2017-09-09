using SyncTool.Git.Common.Groups;
using Xunit;

namespace SyncTool.Git.Test.Common.Groups
{
    public class GitGroupModuleFactoryTest
    {
        [Theory]
        [InlineData(@"https://www.example.com", true)]
        [InlineData(@"http://www.example.com", true)]
        [InlineData(@"C:\MyRepository", true)]
        [InlineData(@"\\server\Repository", true)]
        [InlineData(@"synctool-mysql://server/database", false)]
        public void IsAddressSupported_returns_expected_value(string address, bool expectedvalue)
        {
            var instance = new GitGroupModuleFactory();
            Assert.Equal(expectedvalue, instance.IsAddressSupported(address));
        }
    }
}