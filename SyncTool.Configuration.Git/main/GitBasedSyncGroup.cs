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
using NativeDirectory = System.IO.Directory;
using NativeFile = System.IO.File;

namespace SyncTool.Configuration.Git
{
    public sealed class GitBasedSyncGroup : GitBasedGroup, ISyncGroup
    {
        const string s_SyncFolders = "SyncFolders";
        const string s_Json = "json";

        
        readonly ISyncFolderReader m_SyncFolderReader = new JsonSyncFolderReader();
                

      

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

      

        public GitBasedSyncGroup(string repositoryPath) : base(repositoryPath)
        {            
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
            
        }
        


    }
}