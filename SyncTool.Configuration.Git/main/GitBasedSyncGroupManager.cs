// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using Ninject;
using SyncTool.Configuration.Model;
using SyncTool.FileSystem.Git;
using SyncTool.Utilities;
using NativeDirectory = System.IO.Directory;

namespace SyncTool.Configuration.Git
{
    public sealed class GitBasedSyncGroupManager : ISyncGroupManager
    {
        
        readonly string m_HomeDirectory;


        public IEnumerable<string> SyncGroups
        {
            get
            {
                var directories = GetRepositoryDirectories();
                foreach (var dir in directories)
                {                    
                    using (var group = new GitBasedSyncGroup(dir))
                    {
                        yield return group.Name; 
                    }
                }                
            }
        }

        public ISyncGroup GetSyncGroup(string name)
        {
            var directories = GetRepositoryDirectories();
            foreach (var dir in directories)
            {
                var group = new GitBasedSyncGroup(dir);
                if (group.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    return group;
                }
                else
                {
                    group.Dispose();
                }
            }

            throw new SyncGroupNotFoundException(name);            
        }


        [Inject]
        public GitBasedSyncGroupManager() : this(Environment.CurrentDirectory)
        {
            
        }

        public GitBasedSyncGroupManager(string homeDirectory)
        {
            if (homeDirectory == null)
            {
                throw new ArgumentNullException(nameof(homeDirectory));
            }

            if (NativeDirectory.Exists(homeDirectory) == false)
            {
                throw new DirectoryNotFoundException($"The directory '{homeDirectory}' does not exist");
            }

            m_HomeDirectory = homeDirectory;                        
            
        }


        public ISyncGroup AddSyncGroup(string name)
        {
            if (SyncGroups.Contains(name, StringComparer.CurrentCultureIgnoreCase))
            {
                throw new DuplicateSyncGroupException(name);
            }

            var directoryPath = Path.Combine(m_HomeDirectory, name);

            if (NativeDirectory.Exists(directoryPath))
            {
                throw new ConfigurationException($"Cannot create repository for SyncGroup '{name}'. Directory already exists");
            }

            NativeDirectory.CreateDirectory(directoryPath);
            RepositoryInitHelper.InitializeRepository(directoryPath, name);

            return new GitBasedSyncGroup(directoryPath);
        }

        public void RemoveSyncGroup(string name)
        {
            if (SyncGroups.Contains(name, StringComparer.InvariantCultureIgnoreCase) == false)
            {
                throw new SyncGroupNotFoundException(name);
            }
            
            var directoryPath = Path.Combine(m_HomeDirectory, name);            
            DirectoryHelper.DeleteRecursively(directoryPath);      
        }
      

        public void Dispose()
        {            
        }



        string[] GetRepositoryDirectories() => NativeDirectory.GetDirectories(m_HomeDirectory).Where(IsGitRepository).ToArray();

        bool IsGitRepository(string path)
        {
            try
            {
                using (var repository = new Repository(path))
                {
                    return repository.Info.IsBare;
                }
            }
            catch (RepositoryNotFoundException)
            {
                return false;
            }
        }


    }
}