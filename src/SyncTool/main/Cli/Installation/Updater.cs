using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Squirrel;
using SyncTool.Cli.Configuration;

namespace SyncTool.Cli.Installation
{
    class Updater
    {
        const string s_LastUpdateTimeStampFileName = "lastUpdate.timestamp";

        readonly UpdateConfiguration m_Configuration;
        readonly Task m_UpdateTask;


        public bool IsRunning => !m_UpdateTask.IsCompleted;
        

        public Updater([NotNull] UpdateConfiguration configuration)
        {
            m_Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            m_UpdateTask = CanUpdate() ? StartUpdateTask() : Task.CompletedTask;
        }
        

        public void AwaitCompletion()
        {
            m_UpdateTask.Wait();
        }

        
        bool CanUpdate()
        {
            return ApplicationInfo.IsInstalled &&
                   m_Configuration.Enable &&
                   m_Configuration.Source != UpdateSource.NotConfigured &&
                   !String.IsNullOrEmpty(m_Configuration.Path);
        }
        
        async Task StartUpdateTask()
        {
            var lastUpdateTime = GetLastUpdateTime();
            if (lastUpdateTime.HasValue && (DateTime.UtcNow - lastUpdateTime.Value) < m_Configuration.Interval)
            {
                return;
            }

            switch (m_Configuration.Source)
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
            using (var updateManager = new UpdateManager(m_Configuration.Path))
            {                
                await updateManager.UpdateApp();                
            }
        }
        
        async Task StartGitHubUpdateTask()
        {
            using (var updateManager = await UpdateManager.GitHubUpdateManager(
                repoUrl: m_Configuration.Path, 
                prerelease:m_Configuration.InstallPreReleaseVersions))
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