using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using LibGit2Sharp;

namespace SyncTool.FileSystem.Git
{
    public class GitBasedHistoryManager : IHistoryManager, IDisposable
    {
        const string s_BranchPrefix = "filesystemhistory/";


        readonly string m_RepositoryPath;
        readonly Repository m_Repository;
        readonly ISet<GitBasedFileSystemHistory> m_Histories = new HashSet<GitBasedFileSystemHistory>(); 


        public GitBasedHistoryManager(string repositoryPath)
        {
            m_RepositoryPath = repositoryPath;
            m_Repository = new Repository(repositoryPath);

            foreach (var history in LoadHistories())
            {
                m_Histories.Add(history);
            }

        }


        public IEnumerable<IFileSystemHistory> Histories => m_Histories;

        public IFileSystemHistory CreateHistory(string name)
        {
            var branchName = s_BranchPrefix + name;
            var parentCommit =  m_Repository.Branches["master"].Commits.OrderByDescending(commit => commit.Author.When).First();
            var signature = NewEmptySignature();

            var branch = m_Repository.CreateBranch(branchName, parentCommit, signature);
            
            var newHistory = new GitBasedFileSystemHistory(m_Repository, branch);
            m_Histories.Add(newHistory);

            return newHistory;
        }

        public void Dispose()
        {
            m_Repository.Dispose();
        }



        Signature NewEmptySignature() => new Signature("dummy", "somebody@example.com", DateTimeOffset.Now);


        IEnumerable<GitBasedFileSystemHistory> LoadHistories()
        {
            return m_Repository.Branches.Where(b => !b.IsRemote)
                .Where(b => b.Name.StartsWith(s_BranchPrefix))
                .Select(b => new GitBasedFileSystemHistory(m_Repository, b));            
        } 

    }
}