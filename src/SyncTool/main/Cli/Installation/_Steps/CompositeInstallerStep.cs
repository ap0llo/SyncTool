using System;
using JetBrains.Annotations;

namespace SyncTool.Cli.Installation
{
    class CompositeInstallerStep : IInstallerStep
    {
        readonly IInstallerStep[] m_Steps;


        public CompositeInstallerStep([NotNull] params IInstallerStep[] steps)
        {
            m_Steps = steps ?? throw new ArgumentNullException(nameof(steps));
        }


        public void OnInitialInstall(Version version)
        {
            foreach (var step in m_Steps)
            {
                step.OnInitialInstall(version);
            }
        }

        public void OnAppUpdate(Version version)
        {
            foreach (var step in m_Steps)
            {
                step.OnAppUpdate(version);
            }
        }

        public void OnAppUninstall(Version version)
        {
            foreach (var step in m_Steps)
            {
                step.OnAppUninstall(version);
            }
        }
    }
}