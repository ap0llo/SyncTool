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
using SyncTool.FileSystem.Git;
using SyncTool.Utilities;
using NativeDirectory = System.IO.Directory;

namespace SyncTool.Configuration.Git
{
    public class GitBasedSyncGroupManager : ISyncGroupManager, IDisposable
    {
        readonly CachingObjectMapper<string, GitBasedSyncGroup> m_Mapper;
        readonly string m_HomeDirectory;


        public IEnumerable<ISyncGroup> SyncGroups
        {
            get
            {
                var directories = GetRepositoryDirectories();
                m_Mapper.CleanCache(directories);
                return directories.Select(m_Mapper.MapObject).ToList();
            }
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
            m_Mapper = new CachingObjectMapper<string, GitBasedSyncGroup>((string path) => new GitBasedSyncGroup(path), StringComparer.InvariantCultureIgnoreCase);

        }


        public ISyncGroup CreateSyncGroup(string name)
        {
            if (SyncGroups.Select(x => x.Name).Contains(name, StringComparer.CurrentCultureIgnoreCase))
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

            return m_Mapper.MapObject(directoryPath);
        }

        public void DeleteSyncGroup(string name)
        {
            if (SyncGroups.Select(x => x.Name).Contains(name, StringComparer.InvariantCultureIgnoreCase) == false)
            {
                throw new SyncGroupNotFoundException(name);
            }
            
            var directoryPath = Path.Combine(m_HomeDirectory, name);
            
            // dispose group, so we can delete the directory
            m_Mapper.MapObject(directoryPath).Dispose();

            DirectoryHelper.DeleteRecursively(directoryPath);            
            m_Mapper.CleanCache(GetRepositoryDirectories(), false);
        }
      

        public void Dispose()
        {
            m_Mapper.Dispose();
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