// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using SyncTool.FileSystem.Local;

namespace SyncTool.FileSystem.Git
{
    public abstract class DirectoryBasedTest : IDisposable
    {

        readonly LocalItemCreator m_DirectoryCreator = new LocalItemCreator();
        protected readonly DisposableLocalDirectoryWrapper m_TempDirectory;


        protected DirectoryBasedTest()
        {
            m_TempDirectory = m_DirectoryCreator.CreateTemporaryDirectory();
        }


        public virtual void Dispose()
        {
            m_TempDirectory.Dispose();
        }
    }
}