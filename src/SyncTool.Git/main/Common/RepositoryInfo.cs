using System;
using System.Reflection;

namespace SyncTool.Git.Common
{
    public class RepositoryInfo
    {

        public Version SyncToolVersion { get; set; } = Assembly.GetExecutingAssembly().GetName().Version;

    }
}