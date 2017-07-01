using Autofac;
using SyncTool.Common.Groups;
using SyncTool.Git.DI;

namespace SyncTool.Git.Common.Groups
{
    class GitGroupModuleFactory : IGroupModuleFactory
    {
        public Module CreateModule() => new GitModule();
    }
}
