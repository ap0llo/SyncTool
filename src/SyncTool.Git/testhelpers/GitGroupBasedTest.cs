using SyncTool.Common.TestHelpers;
using SyncTool.Git.DI;
using SyncTool.Git.RepositoryAccess;

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
