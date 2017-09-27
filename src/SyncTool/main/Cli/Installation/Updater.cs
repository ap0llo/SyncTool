using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Squirrel;
using SyncTool.Cli.Options;
using Microsoft.Extensions.Logging;

using static System.FormattableString;

namespace SyncTool.Cli.Installation
{
    class Updater
    {
        const string s_LastUpdateTimeStampFileName = "lastUpdate.timestamp";

        readonly ILogger<Updater> m_Logger;
        readonly UpdateOptions m_Options;
        Task m_UpdateTask;


        public UpdaterStatus Status { get; private set; } = UpdaterStatus.Initialized;
        
        public string Error { get; private set; }


        public Updater([NotNull] ILogger<Updater> logger, [NotNull] UpdateOptions options)
        {
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            m_Options = options ?? throw new ArgumentNullException(nameof(options));            
        }

        public void Start()
        {
            m_Logger.LogDebug("Starting updater");
            Status = UpdaterStatus.Running;

            if (CanUpdate())
            {
                m_UpdateTask = StartUpdateTask();
            }
            else
            {
                m_Logger.LogDebug("Skipping update");
                m_UpdateTask = Task.CompletedTask;
            }

            m_UpdateTask.ContinueWith(t =>
            {
                if (!t.IsFaulted)
                    Status = UpdaterStatus.Completed;

                m_Logger.LogDebug($"Updater completed, Status: {Status} ");
            });
        }
        
        public void Stop()
        {
            try
            {
                m_UpdateTask.Wait();
                Status = UpdaterStatus.Completed;
            }
            catch (AggregateException aggregateException)
            {
                Error = aggregateException
                    .Flatten()
                    .InnerExceptions
                    .Select(e => e.Message)
                    .Aggregate((a, b) => a + "\n" + b);

                Status = UpdaterStatus.Failed;
            }
        }

        
        bool CanUpdate()
        {
            if (!ApplicationInfo.IsInstalled)
            {
                m_Logger.LogDebug("Cannot update, application is not in an installation environment");
                return false;
            }

            if (!m_Options.Enable)
            {
                m_Logger.LogDebug("Cannot update, updates are disabled");
                return false;
            }

            if (m_Options.Source == UpdateSource.NotConfigured)
            {
                m_Logger.LogDebug("Cannot update, update source is not configured");
                return false;
            }

            if (String.IsNullOrEmpty(m_Options.Path))
            {
                m_Logger.LogDebug("Cannot update, update path is not set");
                return false;
            }

            return true;
        }
        
        async Task StartUpdateTask()
        {
            var lastUpdateTime = GetLastUpdateTime();
            if (lastUpdateTime.HasValue && (DateTime.UtcNow - lastUpdateTime.Value) < m_Options.Interval)
            {
                m_Logger.LogDebug(Invariant($"Skipping update, last update check at {lastUpdateTime}"));
                return;
            }
            
            switch (m_Options.Source)
            {
                case UpdateSource.NotConfigured:
                    throw new InvalidOperationException();

                case UpdateSource.GitHub:
                    await StartGitHubUpdateTask();
                    break;


                case UpdateSource.FileSystem:
                    await StartFileSystemUpdateTask();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }


            SetLastUpdateTime();
        }

        async Task StartFileSystemUpdateTask()
        {
            m_Logger.LogDebug($"Updating from file system ({m_Options.Path})");
            using (var updateManager = new UpdateManager(m_Options.Path))
            {                
                await updateManager.UpdateApp();                
            }
        }
        
        async Task StartGitHubUpdateTask()
        {
            m_Logger.LogDebug($"Updating from github ({m_Options.Path})");
            using (var updateManager = await UpdateManager.GitHubUpdateManager(
                repoUrl: m_Options.Path, 
                prerelease:m_Options.InstallPreReleaseVersions))
            {
                await updateManager.UpdateApp();
            }
        }

        DateTime? GetLastUpdateTime()
        {
            var file = GetLastUpdateTimeStampFile();

            if (file.Exists)
            {
                return file.LastWriteTimeUtc;
            }
            else
            {
                return null;
            }
        }

        void SetLastUpdateTime()
        {
            var file = GetLastUpdateTimeStampFile();

            if (!file.Exists)
            {
                using (file.Create()) { }
            }

            file.LastWriteTimeUtc = DateTime.UtcNow;
            file.Refresh();
        }

        FileInfo GetLastUpdateTimeStampFile()
        {
            var path = Path.Combine(ApplicationInfo.RootDirectory, s_LastUpdateTimeStampFileName);
            var fileInfo = new FileInfo(path);
            return fileInfo;
        }
    }

}