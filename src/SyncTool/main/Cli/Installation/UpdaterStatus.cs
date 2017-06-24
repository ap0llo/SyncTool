namespace SyncTool.Cli.Installation
{
    public enum UpdaterStatus
    {
        /// <summary>
        /// The updater has not been started yet
        /// </summary>
        Initialized = 0,

        /// <summary>
        /// The updater is running
        /// </summary>
        Running,

        /// <summary>
        /// The updater completed successfully
        /// </summary>
        Completed,

        /// <summary>
        /// The updater completed with errors
        /// </summary>
        Failed
    }
}