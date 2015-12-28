// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;
using SyncTool.Common;
using SyncTool.FileSystem.Git;
using SyncTool.FileSystem.Git.Utilities;
using NativeDirectory = System.IO.Directory;


namespace SyncTool.FileSystem.Versioning.Git
{
    public sealed class GitBasedHistoryGroup : GitBasedGroup, IHistoryGroup
    {
        const string s_BranchPrefix = "filesystemhistory/";
                


        public GitBasedHistoryGroup(string repositoryPath) : base(repositoryPath)
        {
            
        }



        public IEnumerable<IFileSystemHistory> Items
        {
            get
            {
                return m_Repository.Branches.GetLocalBranches()
                 .Where(b => b.FriendlyName.StartsWith(s_BranchPrefix))
                 .Select(b => new GitBasedFileSystemHistory(m_Repository, b.FriendlyName));
            }
        }

        public void CreateHistory(string name)
        {
            var branchName = s_BranchPrefix + name;
            var parentCommitId = m_Repository.Tags[RepositoryInitHelper.InitialCommitTagName].Target.Sha;
            var parentCommit = m_Repository.Lookup<Commit>(parentCommitId);            
            
            m_Repository.CreateBranch(branchName, parentCommit);            
        }

        public IFileSystemHistory GetItem(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var branchName = s_BranchPrefix + name;
            var branch = m_Repository.GetLocalBranch(branchName);                                     

            if (branch == null)
            {
                throw new ItemNotFoundException(name);
            }
            
            return new GitBasedFileSystemHistory(m_Repository, branch.FriendlyName);
        }



        public static GitBasedHistoryGroup Create(string repositoryLocation)
        {
            if (!NativeDirectory.Exists(repositoryLocation))
            {
                NativeDirectory.CreateDirectory(repositoryLocation);
            }

            RepositoryInitHelper.InitializeRepository(repositoryLocation);

            return new GitBasedHistoryGroup(repositoryLocation);
        }


    }
}