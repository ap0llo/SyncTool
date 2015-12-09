// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using LibGit2Sharp;
using SyncTool.FileSystem.Local;

namespace SyncTool.FileSystem.Git
{
    public sealed class TemporaryWorkingDirectory : IDisposable
    {
        readonly LocalItemCreator m_LocalItemCreator = new LocalItemCreator();
        readonly Repository m_Repository;
        readonly DisposableLocalDirectoryWrapper m_TempDirectory;
        readonly string m_BranchName;

        public string Location => m_TempDirectory.Directory.Location;

        public bool HasChanges => m_Repository.RetrieveStatus().IsDirty;


        public TemporaryWorkingDirectory(string sourceUrl, string branchName)
        {
            m_TempDirectory = m_LocalItemCreator.CreateTemporaryDirectory();

            m_BranchName = branchName;

            Repository.Clone(sourceUrl, m_TempDirectory.Directory.Location, new CloneOptions {BranchName = branchName, Checkout = true});
            m_Repository = new Repository(m_TempDirectory.Directory.Location);
        }


        public string Commit(string commitMessage = "SyncTool Commit")
        {
            StageAllChanges();

            var signature = new Signature("SyncTool", "SyncTool@example.com", DateTimeOffset.Now);

            var commit = m_Repository.Commit(commitMessage, signature, signature);
            return commit.Sha;
        }

        public void Push()
        {            
            m_Repository.Network.Push(m_Repository.Branches[m_BranchName]);
        }

        public void Dispose()
        {
            m_Repository.Dispose();
            m_TempDirectory.Dispose();
        }
        

        void StageAllChanges()
        {
            var status = m_Repository.RetrieveStatus();
            foreach (var statusEntry in status)
            {
                m_Repository.Stage(statusEntry.FilePath);
            }
        }
    }
}