// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;
using SyncTool.FileSystem;
using SyncTool.FileSystem.Local;
using SyncTool.FileSystem.Versioning;
using SyncTool.Git.Common;
using NativeDirectory = System.IO.Directory;
using Path = System.IO.Path;

namespace SyncTool.Git.FileSystem.Versioning
{
    public class GitBasedMultiFileSystemSnapshot : IMultiFileSystemSnapshot
    {
        const string s_SnapshotDirectoryName = "Snapshot";
        const string s_FileNameSuffix = ".SnapshotId.txt";

        readonly Commit m_Commit;
        readonly IHistoryService m_HistoryService;
        readonly Lazy<IDictionary<string, string>> m_SnapshotIds;


        public string Id => m_Commit.Sha;

        public IEnumerable<string> HistoyNames => m_SnapshotIds.Value.Keys;
        

        public GitBasedMultiFileSystemSnapshot(Commit commit, IHistoryService historyService)
        {
            if (commit == null)
                throw new ArgumentNullException(nameof(commit));

            if (historyService == null)
                throw new ArgumentNullException(nameof(historyService));

            if(!IsSnapshot(commit))
                throw new ArgumentException("The specified commit is not a MultiFileSystemSnapshot", nameof(commit));

            m_Commit = commit;
            m_HistoryService = historyService;
            m_SnapshotIds = new Lazy<IDictionary<string, string>>(LoadSnapshotIds);
        }
    

        public IFileSystemSnapshot GetSnapshot(string historyName)
        {
            var snapshotId = m_SnapshotIds.Value[historyName];
            return snapshotId == null ? null : m_HistoryService[historyName][snapshotId];
        }

        public string GetSnapshotId(string historyName) => m_SnapshotIds.Value[historyName];


        public static bool IsSnapshot(Commit commit) => commit.Tree[s_SnapshotDirectoryName] != null;

        public static GitBasedMultiFileSystemSnapshot Create(Repository repository, BranchName branchName, IHistoryService historyService)
        {
            var directoryCreator = new LocalItemCreator();            

            var branch = repository.GetBranch(branchName);

            string commitId;
            using (var workingRepository = new TemporaryWorkingDirectory(repository.Info.Path, branch.FriendlyName))
            {
                var snapshotDirectoryPath = Path.Combine(workingRepository.Location, s_SnapshotDirectoryName);
                if (NativeDirectory.Exists(snapshotDirectoryPath))
                {
                    NativeDirectory.Delete(path: snapshotDirectoryPath, recursive: true);
                }

                var snapshotDirectory = new Directory(null, s_SnapshotDirectoryName);
                foreach (var fileSystemHistory in historyService.Items)
                {
                    var fileName = fileSystemHistory.Name + s_FileNameSuffix;
                    var content = fileSystemHistory.LatestFileSystemSnapshot?.Id ?? "";
                    snapshotDirectory.Add(d => new TextFile(d, fileName, content));
                }
                
                directoryCreator.CreateDirectoryInPlace(snapshotDirectory, snapshotDirectoryPath);

                if (workingRepository.HasChanges)
                {
                    try
                    {
                        commitId = workingRepository.Commit("Created multi-filesystem snapshot");
                        workingRepository.Push();
                    }
                    catch (EmptyCommitException)
                    {
                        // no changes after all (HasChanges does not seem to be a 100% accurate)
                        commitId = repository.GetBranch(branchName).Tip.Sha;
                    }

                }
                else
                {
                    commitId = repository.GetBranch(branchName).Tip.Sha;
                }
            }

            var commit = repository.Lookup<Commit>(commitId);
            return IsSnapshot(commit) ? new GitBasedMultiFileSystemSnapshot(commit, historyService) : null;
        }


        IDictionary<string, string> LoadSnapshotIds()
        {
            var result = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);


            var dir = new GitDirectory(null, "root", m_Commit).Directories.ToArray();

            var files = new GitDirectory(null, "root", m_Commit)
                .GetDirectory(s_SnapshotDirectoryName)
                .Files
                .Where(f => f.Name.EndsWith(s_FileNameSuffix, StringComparison.InvariantCultureIgnoreCase))
                .Cast<IReadableFile>();

            foreach (var file in files)
            {
                var textFile = TextFile.Load(null, file);

                var historyName = file.Name.Replace(s_FileNameSuffix, "");
                var snapshotId = String.IsNullOrEmpty(textFile.Content) ? null : textFile.Content;

                result.Add(historyName, snapshotId);                
            }

            return result;
        } 

    }
}