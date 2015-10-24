using System.IO;

namespace SyncTool.FileSystem
{
    public static class CreateLocalDirectoryVisitorExtensions
    {
        public static TemporaryLocalDirectory CreateTemporaryDirectory(this CreateLocalDirectoryVisitor visitor, IDirectory directory)
        {
            return visitor.CreateDirectory(directory, Path.GetTempPath()).ToTemporaryDirectory();
        }
    }
}