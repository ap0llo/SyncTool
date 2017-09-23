using System;
using System.IO;
using SyncTool.Common.Options;

namespace SyncTool.Common.Groups
{
    class GroupDirectoryPathProvider : IGroupDirectoryPathProvider
    {
        private readonly ApplicationDataOptions m_Options;

        public GroupDirectoryPathProvider(ApplicationDataOptions options)
        {
            m_Options = options ?? throw new ArgumentNullException(nameof(options));
        }
  
        public string GetGroupDirectoryPath(string repositoryName) => 
            Directory.CreateDirectory(Path.Combine(m_Options.RootPath, "Groups", repositoryName)).FullName;       
    }
}