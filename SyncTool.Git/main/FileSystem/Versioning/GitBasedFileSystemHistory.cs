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
            if (fileSystemState == null)
            {
                throw new ArgumentNullException(nameof(fileSystemState));
            }

            var snapshots = m_Snapshots.Value;

            var snapshot = GitBasedFileSystemSnapshot.Create(m_Repository, m_BranchName, this, fileSystemState);

            if (!snapshots.ContainsKey(snapshot.Id))
            {
                snapshots.Add(snapshot.Id, snapshot);
            }
            return snapshot;
        }

        public IFileSystemDiff GetChanges(string toId, string[] pathFilter = null)
        {
            if (toId == null)
            {
                throw new ArgumentNullException(nameof(toId));
            }

            AssertIsValidPathFilter(pathFilter);

            var toSnapshot = GetSnapshot(toId);
            var initialCommit = m_Repository.GetInitialCommit();            

            // build change lists
            var changeLists = GetChangeLists(initialCommit.Sha, toSnapshot, pathFilter);

            return new FileSystemDiff(this, toSnapshot, changeLists);
        }
      
        public IFileSystemDiff GetChanges(string fromId, string toId, string[] pathFilter = null)
        {
            if (fromId == null)
            {
                throw new ArgumentNullException(nameof(fromId));
            }

            if (toId == null)
            {
                throw new ArgumentNullException(nameof(toId));
            }
            
            AssertIsValidPathFilter(pathFilter);

            
            //TODO: Ensure, fromId is an ancestor of toId

            var fromSnapshot = GetSnapshot(fromId);
            var toSnapshot = GetSnapshot(toId);           

            // build change lists
            var changeLists = GetChangeLists(fromSnapshot.Commit.Sha, toSnapshot, null);

            return new FileSystemDiff(this, fromSnapshot, toSnapshot, changeLists);
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
            if (!SnapshotExists(id))
            {
                throw new SnapshotNotFoundException(id);
            }
            return m_Snapshots.Value[id];
        }

        bool SnapshotExists(string id) => m_Snapshots.Value.ContainsKey(id);

        IEnumerable<ChangeList> GetChangeLists(string fromCommit, GitBasedFileSystemSnapshot toSnapshot, string[] paths)
        {
            var changeLists = GetAllChanges(fromCommit, toSnapshot, paths)
                .GroupBy(change => change.Path, StringComparer.InvariantCultureIgnoreCase)
                .Select(group => new ChangeList(group.Reverse()));
                
            return changeLists;
        }

        IEnumerable<IChange> GetAllChanges(string fromCommit, GitBasedFileSystemSnapshot toSnapshot, string[] paths)
        {
            var currentCommit = toSnapshot.Commit;
            var currentSnapshot = toSnapshot;

            // empty path filter => result is empty
            if (paths != null && !paths.Any())
            {
                yield break;
            }

            // paths refers to file in the snapshot, we need to "translate" the paths to the paths in the git repository
            // assumes all paths are rooted
            var pathFilter = paths?.Select(p => GitBasedFileSystemSnapshot.SnapshotDirectoryName + p + FilePropertiesFile.FileNameSuffix).ToArray();
            
            while (currentCommit.Sha != fromCommit)
            {
                var parentCommit = GetParentSnapshotCommit(currentCommit);
                    
                // parent commit is initial commit
                if (parentCommit.Sha == m_Repository.GetInitialCommit().Sha)
                {

                    var treeChanges = m_Repository.Diff.Compare<TreeChanges>(parentCommit.Tree, currentCommit.Tree, pathFilter, null, new CompareOptions() { IncludeUnmodified = false });

                    // build changes
                    foreach (var change in GetChanges(treeChanges, null, currentSnapshot))
                    {
                        yield return change;
                    }

                    // there won't be any commit after this (we already reached the inital commit) 
                    // => abort the loop
                    break;
                }
                else
                {    
                    var parentSnapshot = GetSnapshot(parentCommit.Sha);

                    var treeChanges = m_Repository.Diff.Compare<TreeChanges>(parentCommit.Tree, currentCommit.Tree, pathFilter, null, new CompareOptions() { IncludeUnmodified = false });

                    // build changes
                    foreach(var change in GetChanges(treeChanges, parentSnapshot, currentSnapshot))
                    {
                        yield return change;
                    }

                    currentCommit = parentCommit;
                    currentSnapshot = parentSnapshot;
                }

            }
        }
        
        IEnumerable<IChange> GetChanges(TreeChanges treeChanges, GitBasedFileSystemSnapshot fromSnapshot, GitBasedFileSystemSnapshot toSnapshot)
        {
            foreach (var treeChange in treeChanges)
            {
                if (treeChange.Status == ChangeKind.Unmodified)
                {
                    continue;
                }

                var path = treeChange.Path.Split("\\/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                // ignore changes in the repository outside of the "Snapshot" directory
                var dirName = path.First();
                if (!StringComparer.InvariantCultureIgnoreCase.Equals(dirName, GitBasedFileSystemSnapshot.SnapshotDirectoryName))
                {
                    continue;                    
                }

                // ignore directory property files
                var fileName = path.Last();
                if (StringComparer.InvariantCultureIgnoreCase.Equals(fileName, DirectoryPropertiesFile.FileName))
                {
                    continue;
                }

                switch (treeChange.Status)
                {
                    case ChangeKind.Unmodified:
                        throw new InvalidOperationException("Unmodified changes should have been filtered out");

                    case ChangeKind.Modified:
                        var fromFile = fromSnapshot.GetFileForGitRelativePath(treeChange.Path);
                        var toFile = toSnapshot.GetFileForGitRelativePath(treeChange.Path);
                        yield return new Change(ChangeType.Modified, fromFile.ToReference(), toFile.ToReference());
                        break;                        

                    case ChangeKind.Added:
                        yield return new Change(ChangeType.Added, null, toSnapshot.GetFileForGitRelativePath(treeChange.Path).ToReference());
                        break;

                    case ChangeKind.Deleted:
                        yield return new Change(ChangeType.Deleted, fromSnapshot.GetFileForGitRelativePath(treeChange.Path).ToReference(), null);
                        break;

                    default:
                        throw new NotImplementedException();
                }

            }
        }

        /// <summary>
        /// Gets the first parent of the specified commit that is a snapshot or
        /// the initial commit of the repository if the specified commit is the first snapshot in the history
        /// </summary>
        Commit GetParentSnapshotCommit(Commit commit)
        {
            var currentCommit = commit.Parents.Single();
            var initialCommit = m_Repository.GetInitialCommit();

            while (true)
            {
                if (currentCommit.Sha == initialCommit.Sha || SnapshotExists(currentCommit.Sha))
                {
                    return currentCommit;
                }
                currentCommit = currentCommit.Parents.Single();
            }
        }


        void AssertIsValidPathFilter(string[] pathFilter)
        {
            if (pathFilter == null)
            {
                return;                
            }

            foreach (var path in pathFilter)
            {
                PathValidator.EnsureIsValidFilePath(path);
                PathValidator.EnsureIsRootedPath(path);
            }

        }

    }
}