using System;
using System.IO;
using Xunit;
using SyncTool.Cli.Options;

namespace SyncTool.Test.Options
{
    public class UpdateOptionsTest
    {
        [Fact]
        public void Environment_variables_in_Path_are_expanded_on_set()
        {
            var instance = new UpdateOptions();

            instance.Path = @"https://www.github.com/ap0llo/synctool";
            Assert.Equal(@"https://www.github.com/ap0llo/synctool", instance.Path);

            instance.Path = @"\\some\Path";
            Assert.Equal(@"\\some\Path", instance.Path);

            instance.Path = @"C:\Path";
            Assert.Equal(@"C:\Path", instance.Path);

            instance.Path = @"%TEMP%";
            Assert.Equal(Path.GetTempPath().TrimEnd('\\'), instance.Path.TrimEnd('\\'));

            instance.Path = @"%TEMP%\SyncTool";
            Assert.Equal(Path.Combine(Path.GetTempPath(), "SyncTool"), instance.Path);

            Environment.SetEnvironmentVariable("SYNCTOOL_TEST", "testValue");
            instance.Path = @"%SYNCTOOL_TEST%";
            Assert.Equal("testValue", instance.Path);
        }

    }
}
