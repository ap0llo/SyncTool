// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Instrumentation;
using LibGit2Sharp;
using SyncTool.Configuration.Model;
using SyncTool.Configuration.Reader;
using SyncTool.FileSystem;
using SyncTool.FileSystem.Git;

namespace SyncTool.Configuration.Git
{
    public class GitBasedSyncGroup : ISyncGroup, IDisposable
    {
        const string s_SyncFolders = "SyncFolders";
        const string s_Json = "json";

        readonly Repository m_Repository;
        readonly ISyncFolderReader m_SyncFolderReader = new JsonSyncFolderReader();

        IDictionary<string, SyncFolder> m_Folders;



        public string Name { get; private set; }

        public IEnumerable<SyncFolder> Folders => m_Folders.Values;
     


        public GitBasedSyncGroup(string repositoryPath)
        {
            if (repositoryPath == null)
            {
                throw new ArgumentNullException(nameof(repositoryPath));
            }
            
            m_Repository = new Repository(repositoryPath);

            var directory = new GitDirectory(null, "root", GetConfigurationCommit());
            LoadGroupProperties(directory);
            LoadSyncFolders(directory);

        }

        

        public void Dispose()
        {
            m_Repository.Dispose();
        }



        void LoadGroupProperties(GitDirectory directory)
        {
            var infoFile = (IReadableFile) directory.GetFile(RepositoryInfoFile.RepositoryInfoFileName) ;
            using (var stream = infoFile.OpenRead())
            {
                var repositoryInfo = stream.Deserialize<RepositoryInfo>();
                this.Name = repositoryInfo.RepositoryName;
            }            
        }


        void LoadSyncFolders(GitDirectory directory)
        {
            if (directory.DirectoryExists(s_SyncFolders))
            {                
                var configFiles = directory.GetDirectory(s_SyncFolders).Files.Where(f => f.HasExtensions(s_Json)).Cast<IReadableFile>();

                this.m_Folders = configFiles.Select(file =>
                {
                    using (var stream = file.OpenRead())
                    {
                        return m_SyncFolderReader.ReadSyncFolder(stream);
                    }
                }).ToDictionary(syncFolder => syncFolder.Name, StringComparer.InvariantCultureIgnoreCase);
            }
            else
            {
                m_Folders = new Dictionary<string, SyncFolder>(StringComparer.InvariantCultureIgnoreCase);
            }
        }

        Commit GetConfigurationCommit() => m_Repository.Branches[RepositoryInitHelper.ConfigurationBranchName].Tip;

    }
}