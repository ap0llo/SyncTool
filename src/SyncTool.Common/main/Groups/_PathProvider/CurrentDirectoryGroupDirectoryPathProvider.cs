using System;

namespace SyncTool.Common.Groups
{
    internal class CurrentDirectoryGroupDirectoryPathProvider : SingleDirectoryGroupDirectoryPathProvider
    {
        public CurrentDirectoryGroupDirectoryPathProvider() : base(Environment.CurrentDirectory)
        {
        }
    }
}