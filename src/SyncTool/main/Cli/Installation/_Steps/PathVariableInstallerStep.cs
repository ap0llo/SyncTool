using System;
using System.Collections.Generic;
using System.Linq;

namespace SyncTool.Cli.Installation
{
    public class PathVariableInstallerStep : IInstallerStep
    {
        public void OnInitialInstall(Version version)
        {
            var value = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) ?? "";
            var currentValues = new HashSet<string>(value.Split(';'), StringComparer.InvariantCultureIgnoreCase);

            if (!currentValues.Contains(ApplicationInfo.RootDirectory))
            {
                Environment.SetEnvironmentVariable("PATH", value + ";" + ApplicationInfo.RootDirectory, EnvironmentVariableTarget.User);
            }

        }

        public void OnAppUpdate(Version version)
        {            
        }

        public void OnAppUninstall(Version version)
        {
            var value = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) ?? "";
            var currentValues = new HashSet<string>(value.Split(';'), StringComparer.InvariantCultureIgnoreCase);

            var valuesToRemove = currentValues.Where(v => StringComparer.InvariantCultureIgnoreCase.Equals(v, ApplicationInfo.RootDirectory));
            foreach (var path in valuesToRemove)
            {
                value = value.Replace(path, "");
            }

            while (value.Contains(";;"))
            {
                value = value.Replace(";;", ";");
            }

            Environment.SetEnvironmentVariable("PATH", value, EnvironmentVariableTarget.User);
        }        
    }
}