using System;

namespace SyncTool.Common.Groups
{
    sealed class CurrentDirectoryGroupDirectoryPathProvider : SingleDirectoryGroupDirectoryPathProvider
    {
        public CurrentDirectoryGroupDirectoryPathProvider() : base(Environment.CurrentDirectory)
        {
        }
    }
}