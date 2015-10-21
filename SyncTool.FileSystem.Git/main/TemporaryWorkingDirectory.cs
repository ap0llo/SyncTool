using System;
using LibGit2Sharp;
using SyncTool.Utilities;

namespace SyncTool.FileSystem.Git
{
    public class TemporaryWorkingDirectory : TemporaryDirectory
    {
        readonly Repository m_Repository;
        

        public TemporaryWorkingDirectory(string sourceUrl, string branchName)
        {
            Repository.Clone(sourceUrl, Directory.FullName, new CloneOptions() { BranchName = branchName, Checkout = true});
            m_Repository = new Repository(Directory.FullName);
        }


        public bool HasChanges => m_Repository.RetrieveStatus().IsDirty;


        public void Commit()
        {
            StageAllChanges();

            var signature = new Signature("SyncTool", "synctool@example.com", DateTimeOffset.Now);

            m_Repository.Commit("SyncTool commit", signature, signature);
        }


        public void Push()
        {
            Remote remote = m_Repository.Network.Remotes["origin"];
            m_Repository.Network.Push(remote, @"refs/heads/master");
        }

        public override void Dispose()
        {
            m_Repository.Dispose();            
            base.Dispose();
        }



        void StageAllChanges()
        {
            var status = m_Repository.RetrieveStatus();
            foreach (StatusEntry statusEntry in status)
            {
                m_Repository.Stage(statusEntry.FilePath);
            }            
        }

        

    }
}