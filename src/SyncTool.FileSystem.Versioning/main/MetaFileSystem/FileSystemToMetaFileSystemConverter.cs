using SyncTool.FileSystem;

namespace SyncTool.FileSystem.Versioning.MetaFileSystem
{
    public class FileSystemToMetaFileSystemConverter
    {
        public IDirectory CreateMetaDirectory(IDirectory directory) => CreateMetaDirectory(null, directory);

        public IDirectory CreateMetaDirectory(IDirectory parentDirectory, IDirectory directory)
        {
            var newDirectory = new Directory(parentDirectory, directory.Name);

            foreach (var dir in directory.Directories)
            {
                newDirectory.Add(d => CreateMetaDirectory(d, dir));
            }

            foreach (var file in directory.Files)
            {
                newDirectory.Add(d => FilePropertiesFile.ForFile(d, file));
            }
            
            newDirectory.Add(d => DirectoryPropertiesFile.ForDirectory(d, directory));
            
            return newDirectory;
        }
    }
}