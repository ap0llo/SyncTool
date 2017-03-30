// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
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