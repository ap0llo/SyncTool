using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SyncTool.TestHelpers;
using SyncTool.Git.DI;
using SyncTool.Git.Common;

namespace SyncTool.TestHelpers
{
    public abstract class GitGroupBasedTest : GroupBasedTest<GitModuleFactoryModule>
    {
        public GitGroupBasedTest()
        {
            RepositoryInitHelper.InitializeRepository(m_RemotePath);            
        }

    }
}
