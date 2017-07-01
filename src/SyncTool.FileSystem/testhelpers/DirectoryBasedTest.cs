using System;
using SyncTool.FileSystem.Local;

namespace SyncTool.FileSystem.TestHelpers
{
    public abstract class DirectoryBasedTest : IDisposable
    {
        protected readonly LocalItemCreator m_LocalItemCreator = new LocalItemCreator();
        protected readonly DisposableLocalDirectoryWrapper m_TempDirectory;


        protected DirectoryBasedTest()
        {
            m_TempDirectory = CreateTemporaryDirectory();
        }
        
        public virtual void Dispose()
        {
            m_TempDirectory.Dispose();
        }


        protected DisposableLocalDirectoryWrapper CreateTemporaryDirectory() 
            => m_LocalItemCreator.CreateTemporaryDirectory();
    }
}