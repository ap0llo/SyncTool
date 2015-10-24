//using System;
//using System.Collections.Generic;
//using System.IO;
//using LibGit2Sharp;
//
//namespace SyncTool.FileSystem.Git
//{
//    public class GitBasedFileSystemHistory : IFileSystemHistory
//    {
//        readonly Repository m_Repository;
//        readonly Branch m_Branch;
//        readonly Lazy<IFileSystemSnapshot> m_Latest;
//        readonly Lazy<IEnumerable<IFileSystemSnapshot>> m_States;
//
//
//        public string Id => m_Branch.Name;
//
//        public IFileSystemSnapshot LatestFileSystemSnapshot => m_Latest.Value;
//
//        public IEnumerable<IFileSystemSnapshot> Snapshots => m_States.Value;
//
//
//
//        public GitBasedFileSystemHistory(Repository repository, Branch branch)
//        {
//            if (branch == null)
//            {
//                throw new ArgumentNullException(nameof(branch));
//            }
//            if (repository == null)
//            {
//                throw new ArgumentNullException(nameof(repository));
//            }
//
//            m_Branch = branch;
//            m_Repository = repository;
//
//            m_States = new Lazy<IEnumerable<IFileSystemSnapshot>>(LoadStates);
//            m_Latest = new Lazy<IFileSystemSnapshot>(LoadLatestState);
//        }
//
//
//        public IFileSystemSnapshot CreateSnapshot(Directory fileSystemState) => GitBasedFileSystemSnapshot.Create(m_Repository, m_Branch, fileSystemState);
//
//
//        IEnumerable<IFileSystemSnapshot> LoadStates()
//        {
//            foreach (var commit in m_Branch.Commits)
//            {
//                yield return new GitBasedFileSystemSnapshot(commit);
//            }
//        }
//
//        IFileSystemSnapshot LoadLatestState()
//        {
//            return new GitBasedFileSystemSnapshot(m_Branch.Tip);
//        }
//    }
//}