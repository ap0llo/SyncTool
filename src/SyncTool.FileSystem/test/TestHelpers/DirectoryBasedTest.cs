using System;
using SyncTool.FileSystem.Local;

namespace SyncTool.TestHelpers
{
    public abstract class DirectoryBasedTest : IDisposable
    {
        protected readonly LocalItemCreator m_LocalItemCreator = new LocalItemCreator();
        protected readonly DisposableLocalDirectoryWrapper m_TempDirectory;


        protected DirectoryBasedTest()
        {
            m_TempDirectory = m_LocalItemCreator.CreateTemporaryDirectory();
        }
        
        public virtual void Dispose()
        {
            m_TempDirectory.Dispose();
        }
    }
}