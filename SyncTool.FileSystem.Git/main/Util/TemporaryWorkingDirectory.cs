// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using LibGit2Sharp;
using SyncTool.FileSystem.Local;

namespace SyncTool.FileSystem.Git
{
    public class TemporaryWorkingDirectory : IDisposable
    {
        readonly LocalItemCreator m_LocalItemCreator = new LocalItemCreator();
        readonly Repository m_Repository;
        readonly TemporaryLocalDirectory m_TempDirectory;


        public string Location => m_TempDirectory.Location;

        public bool HasChanges => m_Repository.RetrieveStatus().IsDirty;


        public TemporaryWorkingDirectory(string sourceUrl, string branchName)
        {
            m_TempDirectory = m_LocalItemCreator.CreateTemporaryDirectory();

            Repository.Clone(sourceUrl, m_TempDirectory.Location, new CloneOptions {BranchName = branchName, Checkout = true});
            m_Repository = new Repository(m_TempDirectory.Location);
        }


        public string Commit()
        {
            StageAllChanges();

            var signature = new Signature("SyncTool", "SyncTool@example.com", DateTimeOffset.Now);

            var commit = m_Repository.Commit("SyncTool commit", signature, signature);
            return commit.Sha;
        }

        public void Push()
        {
            var remote = m_Repository.Network.Remotes["origin"];
            m_Repository.Network.Push(remote, @"refs/heads/" + m_Repository.Head.FriendlyName);
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