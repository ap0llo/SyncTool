using System.IO;

namespace SyncTool.FileSystem.Local
{
    public static class CreateLocalDirectoryVisitorExtensions
    {
        public static TemporaryLocalDirectory CreateTemporaryDirectory(this LocalItemCreator visitor, IDirectory directory)
        {
            return visitor.CreateDirectory(directory, Path.GetTempPath()).ToTemporaryDirectory();
        }

        public static TemporaryLocalDirectory CreateTemporaryDirectory(this LocalItemCreator visitor)
        {
            return visitor.CreateTemporaryDirectory(new Directory(Path.GetRandomFileName()));
        }

        public static void CreateDirectoryInPlace(this LocalItemCreator visitor, IDirectory directory, string createIn)
        {
            var name = Path.GetFileName(createIn.Trim("\\//".ToCharArray()));
            createIn = Path.GetDirectoryName(createIn);
            
            visitor.CreateDirectory(new Directory(name, directory.Directories, directory.Files), createIn);            
        }
    }
}