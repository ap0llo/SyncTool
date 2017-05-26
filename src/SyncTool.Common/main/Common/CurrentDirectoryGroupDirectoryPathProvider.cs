using System;

namespace SyncTool.Common
{
    internal class CurrentDirectoryGroupDirectoryPathProvider : SingleDirectoryGroupDirectoryPathProvider
    {
        public CurrentDirectoryGroupDirectoryPathProvider() : base(Environment.CurrentDirectory)
        {
        }
    }
}