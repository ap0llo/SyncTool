using System;
using System.IO;

namespace SyncTool.Common.Options
{
    public sealed class ApplicationDataOptions
    {
        public string RootPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), GetApplicationName());

        static string GetApplicationName()
        {
            var name = AppDomain.CurrentDomain.FriendlyName;
            return name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) 
                ? Path.GetFileNameWithoutExtension(name) 
                : name;
        }
    }
}
