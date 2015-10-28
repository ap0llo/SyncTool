using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;

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

        public static void CreateDirectoryInPlace(this CreateLocalDirectoryVisitor visitor, IDirectory directory, string createIn)
        {
            var name = Path.GetFileName(createIn.Trim("\\//".ToCharArray()));
            createIn = Path.GetDirectoryName(createIn);
            
            visitor.CreateDirectory(new Directory(name, directory.Directories, directory.Files), createIn);            
        }
    }
}