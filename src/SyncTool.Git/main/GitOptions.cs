using System.IO;

namespace SyncTool.Git
{
    public sealed class GitOptions
    {
        public string TempPath { get; set; } = Path.GetTempPath();
    }
}