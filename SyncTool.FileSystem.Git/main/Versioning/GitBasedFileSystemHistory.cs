using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibGit2Sharp;

namespace SyncTool.FileSystem.Git
{
    public class GitBasedFileSystemHistory : IFileSystemHistory
    {
        readonly Repository m_Repository;
        readonly string m_BranchName;
        
        readonly Lazy<IDictionary<string, IFileSystemSnapshot>> m_Snapshots;


        public string Id => m_BranchName;

        public IFileSystemSnapshot LatestFileSystemSnapshot => Snapshots.FirstOrDefault(snapshot => snapshot.Id == m_Repository.Branches[m_BranchName].Tip.Sha);

        public IEnumerable<IFileSystemSnapshot> Snapshots => m_Snapshots.Value.Values;



        public GitBasedFileSystemHistory(Repository repository, string branchName)
        {
            if (branchName == null)
            {
                throw new ArgumentNullException(nameof(branchName));
            }
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            m_BranchName = branchName;
            m_Repository = repository;

            m_Snapshots = new Lazy<IDictionary<string, IFileSystemSnapshot>>(LoadSnapshots);            
        }


        public IFileSystemSnapshot CreateSnapshot(Directory fileSystemState)
        {
            var snapshot = GitBasedFileSystemSnapshot.Create(m_Repository, m_BranchName, fileSystemState);

            if (!m_Snapshots.Value.ContainsKey(snapshot.Id))
            {
                m_Snapshots.Value.Add(snapshot.Id, snapshot);
            }
            return snapshot;
        }


        IDictionary<string, IFileSystemSnapshot> LoadSnapshots() => m_Repository.Branches[m_BranchName].Commits
            .Where(GitBasedFileSystemSnapshot.IsSnapshot)
            .Select(commit => new GitBasedFileSystemSnapshot(commit))
            .Cast<IFileSystemSnapshot>()
            .ToDictionary(snapshot => snapshot.Id, StringComparer.InvariantCultureIgnoreCase);
        
    }
}