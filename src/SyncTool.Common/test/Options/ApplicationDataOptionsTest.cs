using System;
using System.IO;
using Xunit;
using SyncTool.Common.Options;

namespace SyncTool.Common.Test.Options
{
    public class ApplicationDataOptionsTest
    {
        [Fact]
        public void Environment_variables_in_RootPath_are_expanded_on_set()
        {
            var instance = new ApplicationDataOptions();

            instance.RootPath = @"\\some\Path";
            Assert.Equal(@"\\some\Path", instance.RootPath);

            instance.RootPath = @"C:\Path";
            Assert.Equal(@"C:\Path", instance.RootPath);

            instance.RootPath = @"%APPDATA%";
            Assert.Equal(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), instance.RootPath);

            instance.RootPath = @"%APPDATA%\SyncTool";
            Assert.Equal(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SyncTool"), instance.RootPath);

            Environment.SetEnvironmentVariable("SYNCTOOL_TEST", "testValue");
            instance.RootPath = @"%SYNCTOOL_TEST%";
            Assert.Equal("testValue", instance.RootPath);
        }
    }
}
