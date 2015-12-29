// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Functional.Option;
using LibGit2Sharp;
using SyncTool.FileSystem;
using SyncTool.FileSystem.Versioning;
using SyncTool.Git.FileSystem.Versioning.MetaFileSystem;

namespace SyncTool.Git.FileSystem.Versioning
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


        public IFileSystemSnapshot CreateSnapshot(IDirectory fileSystemState)
        {
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

            var changes = treeChanges
                .Where(treeChange => treeChange.Status != ChangeKind.Unmodified)
                .Select(treeChange =>
                {
                    var path = treeChange.Path.Split("\\/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                    // ignore changes in the repository outside of the "Snapshot" directory
                    var dirName = path.First();
                    if (!StringComparer.InvariantCultureIgnoreCase.Equals(dirName, GitBasedFileSystemSnapshot.SnapshotDirectoryName))
                    {
                        return Option.None;
                    }

                    // ignore directory property files
                    var fileName = path.Last();
                    if (StringComparer.InvariantCultureIgnoreCase.Equals(fileName, DirectoryPropertiesFile.FileName))
                    {
                        return Option.None;
                    }


                    switch (treeChange.Status)
                    {
                        case ChangeKind.Unmodified:
                            throw new InvalidOperationException("Unmodified changes should have been filtered out");

                        case ChangeKind.Modified:
                            var fromFile = fromSnapshot.GetFileForGitRelativePath(treeChange.Path);
                            var toFile = toSnapshot.GetFileForGitRelativePath(treeChange.Path);
                            return (Option<Change>)new Change(ChangeType.Modified, fromFile, toFile);                            

                        case ChangeKind.Added:
                            return (Option<Change>)new Change(ChangeType.Added, null, toSnapshot.GetFileForGitRelativePath(treeChange.Path));

                        case ChangeKind.Deleted:
                            return (Option<Change>)new Change(ChangeType.Deleted, fromSnapshot.GetFileForGitRelativePath(treeChange.Path), null); 
                                                   
                        default:
                            throw new NotImplementedException();
                    }
                })
                .Where(change => change.HasValue)
                .Select(change => change.Value)
                .ToList();            

            return new FileSystemDiff(fromSnapshot, toSnapshot, changes);
        }


        IDictionary<string, GitBasedFileSystemSnapshot> LoadSnapshots()
        {
            return m_Repository.Branches[m_BranchName].Commits
                               .Where(GitBasedFileSystemSnapshot.IsSnapshot)
                               .Select(commit => new GitBasedFileSystemSnapshot(commit))
                               .ToDictionary(snapshot => snapshot.Id, StringComparer.InvariantCultureIgnoreCase);
        }

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