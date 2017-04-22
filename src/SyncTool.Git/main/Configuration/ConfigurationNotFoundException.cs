using System;

namespace SyncTool.Git.Configuration
{
    [Serializable]
    public class ConfigurationNotFoundException : Exception
    {

        public ConfigurationNotFoundException(string message) : base(message)
        {
            
        }

    }
}