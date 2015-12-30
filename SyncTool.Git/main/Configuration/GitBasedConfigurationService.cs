// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SyncTool.Common;
using SyncTool.Configuration;
using SyncTool.Configuration.Model;
using SyncTool.Configuration.Reader;
using SyncTool.FileSystem;
using SyncTool.Git.Common;
using NativeDirectory = System.IO.Directory;
using NativeFile = System.IO.File;

namespace SyncTool.Git.Configuration
{
    public sealed class GitBasedConfigurationService : GitBasedService, IConfigurationService
    {
        const string s_SyncFolders = "SyncFolders";
        const string s_Json = "json";

        
        readonly ISyncFolderReader m_SyncFolderReader = new JsonSyncFolderReader();
        

        public SyncFolder this[string name]
        {
            get
            {
                if (String.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentNullException(nameof(name));
                }

                var item = Items.SingleOrDefault(f => f.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                if (item == null)
                {
                    throw new ItemNotFoundException(name);
                }
                return item;
            }
        }

        public IEnumerable<SyncFolder> Items
        {
            get
            {
                var directory = GitGroup.GetConfigurationRootDirectory();
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



        public GitBasedConfigurationService(GitBasedGroup group) : base(group)
        {            
        }


        public void AddSyncFolder(SyncFolder folder)
        {
            if (this.Items.Any(f => f.Name.Equals(folder.Name, StringComparison.CurrentCultureIgnoreCase)))
            {
                throw new DuplicateSyncFolderException(folder.Name);
            }            

            // add config file for the sync folder to the configuration directory
            using (var workingDirectory = new TemporaryWorkingDirectory(GitGroup.Repository.Info.Path, RepositoryInitHelper.ConfigurationBranchName))
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

        public bool ItemExists(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }
            return Items.Any(f => f.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}