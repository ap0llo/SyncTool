// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace SyncTool.FileSystem.Git
{
    public class RepositoryInfo
    {

        public string RepositoryName { get; set; }

        public Version SyncToolVersion { get; set; }


        public RepositoryInfo(string repositoryName)
        {
            if (repositoryName == null)
            {
                throw new ArgumentNullException(nameof(repositoryName));
            }
            
            if (String.IsNullOrWhiteSpace(repositoryName))
            {
                throw new ArgumentException("Repository name must not be empty", nameof(repositoryName));
            }

            SyncToolVersion = Assembly.GetExecutingAssembly().GetName().Version;
            RepositoryName = repositoryName;
        }

        public RepositoryInfo() : this($"Repository_{Guid.NewGuid()}")
        {
            
        }

    }
}