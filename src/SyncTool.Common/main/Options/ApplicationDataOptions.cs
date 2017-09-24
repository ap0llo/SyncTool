using System;
using System.IO;

namespace SyncTool.Common.Options
{
    public sealed class ApplicationDataOptions
    {
        string m_RootPath;


        public string RootPath
        {
            get => m_RootPath;
            set => m_RootPath = Environment.ExpandEnvironmentVariables(value);
        }


        public ApplicationDataOptions()
        {
            RootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), GetApplicationName());
        }


        static string GetApplicationName()
        {
            var name = AppDomain.CurrentDomain.FriendlyName;
            return name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) 
                ? Path.GetFileNameWithoutExtension(name) 
                : name;
        }
    }
}
