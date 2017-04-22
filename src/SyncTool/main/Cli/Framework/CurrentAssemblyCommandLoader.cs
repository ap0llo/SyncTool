using System.Collections.Generic;
using System.Reflection;

namespace SyncTool.Cli.Framework
{
    public class CurrentAssemblyCommandLoader : AbstractCommandLoader
    {
        protected override IEnumerable<Assembly> GetCommandAssemblies()
        {
            yield return Assembly.GetExecutingAssembly();
        }
    }
}