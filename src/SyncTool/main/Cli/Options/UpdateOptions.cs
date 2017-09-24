using System;

namespace SyncTool.Cli.Options
{
    enum UpdateSource
    {
        NotConfigured = 0,
        GitHub = 1,
        FileSystem = 2
    }

    sealed class UpdateOptions
    {
        string m_Path;

        public bool Enable { get; set; } = false;

        public UpdateSource Source { get; set; } = UpdateSource.NotConfigured;

        public string Path
        {
            get => m_Path;
            set => m_Path = Environment.ExpandEnvironmentVariables(value);
        }

        public bool InstallPreReleaseVersions { get; set; } = false;

        public TimeSpan Interval { get; set; } = TimeSpan.FromHours(1);


        public UpdateOptions()
        {
            Path = "";
        }

    }    
}