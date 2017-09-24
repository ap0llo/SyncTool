using System;
using System.IO;
using Xunit;
using SyncTool.Git.Options;

namespace SyncTool.Git.Test.Options
{
    public class GitOptionsTest
    {
        [Fact]
        public void Environment_variables_in_TempPath_are_expanded_on_set()
        {
            var instance = new GitOptions();

            instance.TempPath = @"\\some\Path";
            Assert.Equal(@"\\some\Path", instance.TempPath);

            instance.TempPath = @"C:\Path";
            Assert.Equal(@"C:\Path", instance.TempPath);

            instance.TempPath = @"%TEMP%";
            Assert.Equal(Path.GetTempPath().TrimEnd('\\'), instance.TempPath.TrimEnd('\\'));

            instance.TempPath = @"%TEMP%\SyncTool";
            Assert.Equal(Path.Combine(Path.GetTempPath(), "SyncTool"), instance.TempPath);

            Environment.SetEnvironmentVariable("SYNCTOOL_TEST", "testValue");
            instance.TempPath = @"%SYNCTOOL_TEST%";
            Assert.Equal("testValue", instance.TempPath);
        }
    }
}
