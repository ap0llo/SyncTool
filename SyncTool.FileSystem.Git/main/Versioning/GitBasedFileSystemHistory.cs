using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using SyncTool.FileSystem.Versioning;

namespace SyncTool.FileSystem.Git
{
    public class GitBasedFileSystemHistory : IFileSystemHistory
    {
        readonly Repository m_Repository;
        readonly string m_BranchName;
        
        readonly Lazy<IDictionary<string, GitBasedFileSystemSnapshot>> m_Snapshots;


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

            m_Snapshots = new Lazy<IDictionary<string, GitBasedFileSystemSnapshot>>(LoadSnapshots);            
        }


        public IFileSystemSnapshot CreateSnapshot(Directory fileSystemState)
        {
            Console.WriteLine("Creating Snapshot");

            var snapshots = m_Snapshots.Value;

            var snapshot = GitBasedFileSystemSnapshot.Create(m_Repository, m_BranchName, fileSystemState);

            if (!snapshots.ContainsKey(snapshot.Id))
            {
                snapshots.Add(snapshot.Id, snapshot);
            }
            return snapshot;
        }

        public IFileSystemDiff CompareSnapshots(string fromId, string toId)
        {
            var fromSnapshot = GetSnapshot(fromId);
            var toSnapshot = GetSnapshot(toId);
            
            var treeChanges = m_Repository.Diff.Compare<TreeChanges>(fromSnapshot.Commit.Tree, toSnapshot.Commit.Tree, null, null, new CompareOptions() { IncludeUnmodified = false });

            //TODO: Convert treeChanges to list of IChange

            return new FileSystemDiff(fromSnapshot, toSnapshot, Enumerable.Empty<IChange>());

        }


        IDictionary<string, GitBasedFileSystemSnapshot> LoadSnapshots() => m_Repository.Branches[m_BranchName].Commits
            .Where(GitBasedFileSystemSnapshot.IsSnapshot)
            .Select(commit => new GitBasedFileSystemSnapshot(commit))            
            .ToDictionary(snapshot => snapshot.Id, StringComparer.InvariantCultureIgnoreCase);


        GitBasedFileSystemSnapshot GetSnapshot(string id)
        {
            if (!m_Snapshots.Value.ContainsKey(id))
            {
                throw new SnapshotNotFoundException(id);
            }
            return m_Snapshots.Value[id];
        }

        
    }
}