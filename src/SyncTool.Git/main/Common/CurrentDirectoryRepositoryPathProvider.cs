using System;
using SyncTool.Git.FileSystem;

namespace SyncTool.Git.Common
{
    public class CurrentDirectoryRepositoryPathProvider : SingleDirectoryRepositoryPathProvider
    {
        public CurrentDirectoryRepositoryPathProvider() : base(Environment.CurrentDirectory)
        {
        }
    }
}