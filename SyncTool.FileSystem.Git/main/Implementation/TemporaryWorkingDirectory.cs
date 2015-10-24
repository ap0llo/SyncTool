using System;
using LibGit2Sharp;

namespace SyncTool.FileSystem.Git
{
    public class TemporaryWorkingDirectory : IDisposable
    {
        readonly CreateLocalDirectoryVisitor m_CreateLocalDirectoryVisitor = new CreateLocalDirectoryVisitor();
        readonly Repository m_Repository;
        readonly TemporaryLocalDirectory m_TempDirectory;


        public string Location => m_TempDirectory.Location;

        public bool HasChanges => m_Repository.RetrieveStatus().IsDirty;


        public TemporaryWorkingDirectory(string sourceUrl, string branchName)
        {
            m_TempDirectory = m_CreateLocalDirectoryVisitor.CreateTemporaryDirectory();

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
            m_Repository.Network.Push(remote, @"refs/heads/" + m_Repository.Head.Name);
        }

        public void Dispose()
        {
            m_TempDirectory.Dispose();
            m_Repository.Dispose();
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