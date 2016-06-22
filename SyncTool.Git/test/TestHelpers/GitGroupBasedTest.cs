// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using SyncTool.FileSystem;
using SyncTool.Git.Common;
using SyncTool.TestHelpers;
using Directory = System.IO.Directory;

namespace SyncTool.Git.TestHelpers
{
    /// <summary>
    /// Test base class for tests that require a <see cref="GitBasedGroup"/>
    /// </summary>
    public abstract class GitGroupBasedTest : DirectoryBasedTest
    {
        protected readonly string m_RemotePath;
        readonly IRepositoryPathProvider m_PathProvider;

        protected GitGroupBasedTest()
        {
            m_RemotePath = Path.Combine(m_TempDirectory.Location, "Remote");
            Directory.CreateDirectory(m_RemotePath);
            RepositoryInitHelper.InitializeRepository(m_RemotePath);

            var localPath = Path.Combine(m_TempDirectory.Location, "Local");
            Directory.CreateDirectory(localPath);
            m_PathProvider = new SingleDirectoryRepositoryPathProvider(localPath);
        }


        protected GitBasedGroup CreateGroup()
        {
            return new GitBasedGroup(EqualityComparer<IFileReference>.Default, m_PathProvider, "Irrelevant", m_RemotePath);
        }

    }
}