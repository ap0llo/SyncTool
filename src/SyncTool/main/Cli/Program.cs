using Ninject;
using SyncTool.Cli.DI;
using SyncTool.Cli.Framework;
using SyncTool.Git.DI;
using SyncTool.Synchronization.DI;

namespace SyncTool.Cli
{
    class Program
    {
        static int Main(string[] args)
        {
            var kernel = new StandardKernel(new CliModule(), new GitModule(), new SynchronizationModule());
            return kernel.Get<Application>().Run(args);
        }
    }
}