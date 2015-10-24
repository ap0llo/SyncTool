using System.IO;

namespace SyncTool.FileSystem
{
    public static class CreateLocalDirectoryVisitorExtensions
    {
        public static TemporaryLocalDirectory CreateTemporaryDirectory(this CreateLocalDirectoryVisitor visitor, IDirectory directory)
        {
            return visitor.CreateDirectory(directory, Path.GetTempPath()).ToTemporaryDirectory();
        }

        public static TemporaryLocalDirectory CreateTemporaryDirectory(this CreateLocalDirectoryVisitor visitor)
        {
            return visitor.CreateTemporaryDirectory(new Directory(Path.GetRandomFileName()));
        }
    }
}