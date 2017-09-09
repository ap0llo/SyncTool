using SyncTool.Sql.DI;
using Xunit;

namespace SyncTool.Sql.Test
{
    public class SqlGroupModuleFactoryTest
    {
        [Theory]
        [InlineData(@"https://someUrl", false)]       
        [InlineData(@"mysql://someUrl", false)]       
        [InlineData(@"http://someUrl", false)]       
        [InlineData(@"\\somepath", false)]       
        [InlineData(@"C:\some\path", false)]       
        [InlineData(@"synctool-mysql://someUrl", true)]       
        public void IsAddressSupported_returns_expected_value(string address, bool expectedValue)
        {
            var instance = new SqlGroupModuleFactory();
            Assert.Equal(expectedValue, instance.IsAddressSupported(address));
        }
    }
}