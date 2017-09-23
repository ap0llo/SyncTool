using System.IO;

namespace SyncTool.Git.Options
{
    public sealed class GitOptions
    {
        public string TempPath { get; set; } = Path.GetTempPath();
    }
}