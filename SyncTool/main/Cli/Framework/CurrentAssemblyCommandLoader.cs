// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

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