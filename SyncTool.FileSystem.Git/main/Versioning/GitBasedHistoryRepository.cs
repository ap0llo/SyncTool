﻿// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;
using SyncTool.FileSystem.Versioning;
using NativeDirectory = System.IO.Directory;


namespace SyncTool.FileSystem.Git
{
    public class GitBasedHistoryRepository : IHistoryRepository, IDisposable
    {
        const string s_BranchPrefix = "filesystemhistory/";

        readonly Repository m_Repository;
        readonly ISet<GitBasedFileSystemHistory> m_Histories = new HashSet<GitBasedFileSystemHistory>(); 


        public GitBasedHistoryRepository(string repositoryPath)
        {
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
            var parentCommitId = m_Repository.Tags[RepositoryInitHelper.InitialCommitTagName].Target.Sha;
            var parentCommit = m_Repository.Lookup<Commit>(parentCommitId);            
            
            m_Repository.CreateBranch(branchName, parentCommit);

            var newHistory = new GitBasedFileSystemHistory(m_Repository, branchName);
            m_Histories.Add(newHistory);

            return newHistory;
        }

        public void Dispose()
        {
            m_Repository.Dispose();
        }


        public static GitBasedHistoryRepository Create(string repositoryLocation)
        {
            if (!NativeDirectory.Exists(repositoryLocation))
            {
                NativeDirectory.CreateDirectory(repositoryLocation);
            }

            RepositoryInitHelper.InitializeRepository(repositoryLocation);

            return new GitBasedHistoryRepository(repositoryLocation);
        }


        
        IEnumerable<GitBasedFileSystemHistory> LoadHistories()
        {
            return m_Repository.Branches.GetLocalBranches()
                .Where(b => b.FriendlyName.StartsWith(s_BranchPrefix))
                .Select(b => new GitBasedFileSystemHistory(m_Repository, b.FriendlyName));            
        } 

    }
}