using Ninject.Modules;
using SyncTool.Common;
using SyncTool.Git.Common;
using SyncTool.Git.Configuration.Reader;

namespace SyncTool.Git.DI
{
    public class GitModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IGroupSettingsProvider>().To<JsonGroupSettingsProvider>();
            this.Bind<IRepositoryPathProvider>().To<CurrentDirectoryRepositoryPathProvider>();
            this.Bind<IGroupManager>().To<GitBasedGroupManager>();
        }
    }
}