using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Squirrel;
using SyncTool.Cli.Configuration;

namespace SyncTool.Cli.Update
{
    class Updater
    {
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
            return m_Configuration.EnableAutoUpdate &&
                   m_Configuration.UpdateSource != UpdateSource.NotConfigured &&
                   !String.IsNullOrEmpty(m_Configuration.UpdatePath);
        }


        Task StartUpdateTask()
        {
            switch (m_Configuration.UpdateSource)
            {
                case UpdateSource.NotConfigured:
                    throw new InvalidOperationException();

                case UpdateSource.GitHub:
                    return StartGitHubUpdateTask();                    

                case UpdateSource.FileSystem:
                    return StartFileSystemUpdateTask();                    

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        async Task StartFileSystemUpdateTask()
        {        
            using (var updateManager = new UpdateManager(m_Configuration.UpdatePath))
            {                
                await updateManager.UpdateApp();                
            }
        }
        
        async Task StartGitHubUpdateTask()
        {
            using (var updateManager = await UpdateManager.GitHubUpdateManager(m_Configuration.UpdatePath))
            {
                await updateManager.UpdateApp();
            }
        }
    }

}