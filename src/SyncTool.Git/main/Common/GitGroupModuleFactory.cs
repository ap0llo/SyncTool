using SyncTool.Common.Groups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using SyncTool.Git.DI;

namespace SyncTool.Git.Common
{
    class GitGroupModuleFactory : IGroupModuleFactory
    {
        public Module CreateModule()
        {
            return new GitModule();
        }
    }
}
