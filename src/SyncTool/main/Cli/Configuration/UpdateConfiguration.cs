namespace SyncTool.Cli.Configuration
{
    class UpdateConfiguration
    {
        public bool EnableAutoUpdate { get; set; } = false;

        public UpdateSource UpdateSource { get; set; } = UpdateSource.NotConfigured;

        public string UpdatePath { get; set; } = "";

        public bool InstallPreReleaseVersions { get; set; } = false;
    }
    
}