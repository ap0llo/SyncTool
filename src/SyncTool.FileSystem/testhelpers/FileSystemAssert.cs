using System.Linq;
using Xunit;

namespace SyncTool.FileSystem.TestHelpers
{
    /// <summary>
    /// Provides assertions for SyncTool.FileSystem types 
    /// </summary>
    public static class FileSystemAssert
    {
        public static void DirectoryEqual(IDirectory expected, IDirectory actual)
        {
            Assert.Equal(expected.Path, actual.Path);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.Directories.Count(), actual.Directories.Count());
            Assert.Equal(expected.Files.Count(), actual.Files.Count());

            foreach (var directory in expected.Directories)
            {
                Assert.True(actual.DirectoryExists(directory.Name));
                DirectoryEqual(directory, actual.GetDirectory(directory.Name));
            }

            foreach (var file in expected.Files)
            {
                Assert.True(actual.FileExists(file.Name));
                FileEqual(file, actual.GetFile(file.Name));
            }
        }

        public static void FileEqual(IFile expected, IFile actual)
        {
            Assert.Equal(expected.Path, actual.Path);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.Length, actual.Length);
            Assert.Equal(expected.LastWriteTime, actual.LastWriteTime);
        }

        public static void FileReferenceMatches(IFile file, FileReference reference)
        {
            Assert.True(reference.Matches(file));            
        }
    }
}