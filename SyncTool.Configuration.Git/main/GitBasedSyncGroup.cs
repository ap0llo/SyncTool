// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using SyncTool.Configuration.Model;
using SyncTool.Configuration.Reader;
using SyncTool.FileSystem;
using SyncTool.FileSystem.Git;
using SyncTool.FileSystem.Versioning;
using SyncTool.FileSystem.Versioning.Git;
using NativeDirectory = System.IO.Directory;
using NativeFile = System.IO.File;

namespace SyncTool.Configuration.Git
{
    public sealed class GitBasedSyncGroup : ISyncGroup, IDisposable
    {
        const string s_SyncFolders = "SyncFolders";
        const string s_Json = "json";

        readonly Repository m_Repository;
        readonly ISyncFolderReader m_SyncFolderReader = new JsonSyncFolderReader();
        readonly IHistoryRepository m_HistoryRepository; 



        public string Name
        {
            get
            {
                var infoFile = (IReadableFile)GetConfigurationRootDirectory().GetFile(RepositoryInfoFile.RepositoryInfoFileName);
                using (var stream = infoFile.OpenRead())
                {
                    var repositoryInfo = stream.Deserialize<RepositoryInfo>();
                    return repositoryInfo.RepositoryName;
                }
            }
        }

        public IEnumerable<SyncFolder> Folders
        {
            get
            {
                var directory = GetConfigurationRootDirectory();
                if (directory.DirectoryExists(s_SyncFolders))
                {
                    var configFiles = directory.GetDirectory(s_SyncFolders).Files.Where(f => f.HasExtensions(s_Json)).Cast<IReadableFile>();

                    return configFiles.Select(file =>
                    {
                        using (var stream = file.OpenRead())
                        {
                            return m_SyncFolderReader.ReadSyncFolder(stream);
                        }
                    }).ToList();
                }
                else
                {
                    return Enumerable.Empty<SyncFolder>();
                }
            }
        }

        public SyncFolder this[string name] => Folders.Single(f => f.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

      

        public GitBasedSyncGroup(string repositoryPath)
        {
            if (repositoryPath == null)
            {
                throw new ArgumentNullException(nameof(repositoryPath));
            }
            
            m_Repository = new Repository(repositoryPath);
            m_HistoryRepository = new GitBasedHistoryRepository(repositoryPath);
            

        }


        public void AddSyncFolder(SyncFolder folder)
        {
            if (this.Folders.Any(f => f.Name.Equals(folder.Name, StringComparison.CurrentCultureIgnoreCase)))
            {
                throw new DuplicateSyncFolderException(folder.Name);
            }            

            // add config file for the sync folder to the configuration directory
            using (var workingDirectory = new TemporaryWorkingDirectory(m_Repository.Info.Path, RepositoryInitHelper.ConfigurationBranchName))
            {
                var syncFoldersPath = Path.Combine(workingDirectory.Location, s_SyncFolders);

                if (NativeDirectory.Exists(syncFoldersPath) == false)
                {
                    NativeDirectory.CreateDirectory(syncFoldersPath);
                }

                var filePath = Path.Combine(syncFoldersPath, $"{folder.Name}.{s_Json}");
                using (var stream = NativeFile.Open(filePath, FileMode.CreateNew, FileAccess.Write))
                {
                    folder.WriteTo(stream);
                }

                workingDirectory.Commit($"Added SyncFolder '{folder.Name}'");
                workingDirectory.Push();
            }

            // create a history for the sync folder
            m_HistoryRepository.CreateHistory(folder.Name);
        }

        public IFileSystemHistory GetHistory(string syncFolderName)
        {
            return m_HistoryRepository.GetHistory(syncFolderName);
        }

        public void Dispose()
        {
            m_Repository.Dispose();
            m_HistoryRepository.Dispose();
        }





        Commit GetConfigurationCommit() => m_Repository.Branches[RepositoryInitHelper.ConfigurationBranchName].Tip;

        GitDirectory GetConfigurationRootDirectory() => new GitDirectory(null, "root", GetConfigurationCommit());

    }
}