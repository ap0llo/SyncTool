using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using SyncTool.Common.Services;
using SyncTool.Configuration;
using SyncTool.FileSystem;
using SyncTool.Git.Common.Services;
using SyncTool.Git.RepositoryAccess;

using NativeDirectory = System.IO.Directory;
using NativeFile = System.IO.File;

namespace SyncTool.Git.Configuration
{
    public sealed class GitBasedConfigurationService : GitBasedService, IConfigurationService
    {
        const string s_SyncFolders = "SyncFolders";
        const string s_Json = "json";               
        

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
                    throw new ItemNotFoundException($"An item named '{name}' was not found");
                }
                return item;
            }
        }

        public IEnumerable<SyncFolder> Items
        {
            get
            {
                var directory = Repository.GetConfigurationRootDirectory();
                if (directory.DirectoryExists(s_SyncFolders))
                {
                    var configFiles = directory.GetDirectory(s_SyncFolders).Files.Where(f => f.HasExtension(s_Json)).Cast<IReadableFile>();

                    return configFiles.Select(file =>
                    {
                        using (var stream = file.OpenRead())
                        {
                            return stream.Deserialize<SyncFolder>();
                        }
                    }).ToList();
                }
                else
                {
                    return Enumerable.Empty<SyncFolder>();
                }
            }
        }


        public GitBasedConfigurationService(GitRepository repository) : base(repository)
        {            
        }


        public void AddItem(SyncFolder folder)
        {
            if (this.Items.Any(f => f.Name.Equals(folder.Name, StringComparison.CurrentCultureIgnoreCase)))
            {
                throw new DuplicateSyncFolderException(folder.Name);
            }            

            // add config file for the sync folder to the configuration directory
            using (var workingDirectory = new TemporaryWorkingDirectory(Repository.Value.Info.Path, RepositoryInitHelper.ConfigurationBranchName.ToString()))
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

        public void UpdateItem(SyncFolder folder)
        {
            if (folder == null)
            {
                throw new ArgumentNullException(nameof(folder));
            }

            if (!ItemExists(folder.Name))
            {
                throw new SyncFolderNotFoundException($"A sync folder named '{folder.Name}' could not be found");
            }

            using (var workingDirectory = new TemporaryWorkingDirectory(Repository.Value.Info.Path, RepositoryInitHelper.ConfigurationBranchName.ToString()))
            {
                var syncFoldersPath = Path.Combine(workingDirectory.Location, s_SyncFolders);
                
                var filePath = Path.Combine(syncFoldersPath, $"{folder.Name}.{s_Json}");
                using (var stream = NativeFile.Open(filePath, FileMode.Open, FileAccess.Write))
                {
                    folder.WriteTo(stream);
                }

                if (workingDirectory.HasChanges)
                {
                    try
                    {
                        workingDirectory.Commit($"Updated SyncFolder '{folder.Name}'");
                        workingDirectory.Push();
                    }
                    catch (EmptyCommitException)
                    {
                        // no changes after all (HasChanges does not seem to be a 100% accurate)
                        // => ignore exception
                    }

                    
                }

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
    }
}