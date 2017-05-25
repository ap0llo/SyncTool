using System;

namespace SyncTool.Common
{
    public class CurrentDirectoryGroupDirectoryPathProvider : SingleDirectoryGroupDirectoryPathProvider
    {
        public CurrentDirectoryGroupDirectoryPathProvider() : base(Environment.CurrentDirectory)
        {
        }
    }
}