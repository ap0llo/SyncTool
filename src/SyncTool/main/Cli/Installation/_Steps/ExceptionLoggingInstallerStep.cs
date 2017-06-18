using System;
using System.IO;
using JetBrains.Annotations;

namespace SyncTool.Cli.Installation
{
    class ExceptionLoggingInstallerStep : IInstallerStep
    {
        readonly IInstallerStep m_InnerStep;


        public ExceptionLoggingInstallerStep([NotNull] IInstallerStep innerStep)
        {
            m_InnerStep = innerStep ?? throw new ArgumentNullException(nameof(innerStep));
        }


        public void OnInitialInstall(Version version)
        {
            Execute(() => m_InnerStep.OnInitialInstall(version));
        }

        public void OnAppUpdate(Version version)
        {
            Execute(() => m_InnerStep.OnAppUpdate(version));
        }

        public void OnAppUninstall(Version version)
        {
            Execute(() => m_InnerStep.OnAppUninstall(version));
        }


        static void Execute(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {

                LogException(e);
                throw;
            }
        }

        static void LogException(Exception e)
        {
            try
            {
                using (var stream = File.OpenWrite(Path.Combine(ApplicationInfo.CurrentVersionRootDirectory, $"Exception_{Guid.NewGuid()}.txt")))
                using (var writer = new StreamWriter(stream))
                {
                    writer.WriteLine(DateTime.Now);
                    writer.Write(e);
                }
            }
            catch
            {
                // ignore
            }
        }
    }
}