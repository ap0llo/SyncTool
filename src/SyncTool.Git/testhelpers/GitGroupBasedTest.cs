using SyncTool.Common.TestHelpers;
using SyncTool.Git.Common;
using SyncTool.Git.DI;

namespace SyncTool.Git.TestHelpers
{
    public abstract class GitGroupBasedTest : GroupBasedTest<GitModuleFactoryModule>
    {
        protected GitGroupBasedTest()
        {
            RepositoryInitHelper.InitializeRepository(m_RemotePath);            
        }   
    }
}
