// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Functional.Option;
using LibGit2Sharp;
using SyncTool.FileSystem;
using SyncTool.FileSystem.Versioning;
using SyncTool.Git.Common;
using SyncTool.Git.FileSystem.Versioning.MetaFileSystem;

namespace SyncTool.Git.FileSystem.Versioning
{
    public class GitBasedFileSystemHistory : IFileSystemHistory
    {
        public const string BranchNamePrefix = "filesystemHistory";

        readonly Repository m_Repository;
        readonly BranchName m_BranchName;
        
        readonly Lazy<IDictionary<string, GitBasedFileSystemSnapshot>> m_Snapshots;


        public string Name => m_BranchName.Name;

        public string Id => m_BranchName.ToString();

        public IFileSystemSnapshot LatestFileSystemSnapshot => Snapshots.FirstOrDefault(snapshot => snapshot.Id == m_Repository.GetBranch(m_BranchName).Tip.Sha);

        public IEnumerable<IFileSystemSnapshot> Snapshots => m_Snapshots.Value.Values;


        public GitBasedFileSystemHistory(Repository repository, BranchName branchName)
        {
            if (branchName == null)
            {
                throw new ArgumentNullException(nameof(branchName));
            }
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            m_Repository = repository;
            var branch = repository.GetBranch(branchName);
            m_BranchName = BranchName.Parse(branch.FriendlyName);

            m_Snapshots = new Lazy<IDictionary<string, GitBasedFileSystemSnapshot>>(LoadSnapshots);
        }

        public GitBasedFileSystemHistory(Repository repository, string historyName) : this(repository, new BranchName(BranchNamePrefix, historyName))
        {
                 
        }


        public IFileSystemSnapshot CreateSnapshot(IDirectory fileSystemState)
        {
            var snapshots = m_Snapshots.Value;

            var snapshot = GitBasedFileSystemSnapshot.Create(m_Repository, m_BranchName, this, fileSystemState);

            if (!snapshots.ContainsKey(snapshot.Id))
            {
                snapshots.Add(snapshot.Id, snapshot);
            }
            return snapshot;
        }

        public IFileSystemDiff GetChanges(string toId)
        {
            var toSnapshot = GetSnapshot(toId);

            var treeChanges = m_Repository.Diff.Compare<TreeChanges>(m_Repository.GetInitialCommit().Tree, toSnapshot.Commit.Tree, null, null, new CompareOptions() { IncludeUnmodified = false });

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

                        //since we're getting all changes from the initial commit (which is empty) to the snapshot
                        // only additions of files are possible
                        case ChangeKind.Added:
                            return (Option<Change>)new Change(ChangeType.Added, null, toSnapshot.GetFileForGitRelativePath(treeChange.Path));
                        
                        default:
                            throw new NotImplementedException();
                    }
                })
                .Where(change => change.HasValue)
                .Select(change => change.Value)
                .ToList();


            return new FileSystemDiff(this, toSnapshot, changes);
        }

        public IFileSystemDiff GetChanges(string fromId, string toId)
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

            return new FileSystemDiff(this, fromSnapshot, toSnapshot, changes);
        }


        IDictionary<string, GitBasedFileSystemSnapshot> LoadSnapshots()
        {
            return m_Repository.GetBranch(m_BranchName).Commits
                               .Where(GitBasedFileSystemSnapshot.IsSnapshot)
                               .Select(commit => new GitBasedFileSystemSnapshot(this, commit))
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