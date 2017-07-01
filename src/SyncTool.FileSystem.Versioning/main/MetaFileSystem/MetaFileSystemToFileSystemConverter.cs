namespace SyncTool.FileSystem.Versioning.MetaFileSystem
{
    /// <summary>    
    /// Converter that takes a "meta file system" and retrieves the original file system stored in there 
    /// </summary>
    public class MetaFileSystemToFileSystemConverter
    {
        public IDirectory Convert(IDirectory directory) => ConvertDynamic(null, directory);

        
        IDirectory Convert(IDirectory newParent, IDirectory toConvert)
        {            
            // determine name of new directory
            var newDirectoryName = toConvert.Name;
            foreach (var file in toConvert.Files)
            {
                var name = GetDirectoryNameDynamic(file);
                if (name != null)
                {
                    newDirectoryName = name;
                    break;
                }
            }
           
            var newDirectory = new Directory(newParent, newDirectoryName);            

            foreach (var childDirectory in toConvert.Directories)
            {
                var newChildDirectory = (IDirectory) ConvertDynamic(newDirectory, childDirectory);
                if (newChildDirectory != null)
                {
                    newDirectory.Add(_ => newChildDirectory);
                }
            }

            foreach (var file in toConvert.Files)
            {
                var newFile = (IFile) ConvertDynamic(newDirectory, file);
                if (newFile != null)
                {
                    newDirectory.Add(_ => newFile);
                }
            }

            return newDirectory;
        }

        IFile Convert(IDirectory newParent, FilePropertiesFile file)
        {
            // load file properties            
            var newFile = new File(newParent, file.Content.Name) { LastWriteTime =  file.Content.LastWriteTime, Length = file.Content.Length };            
            return newFile;
        }

        IFile Convert(IDirectory newParent, DirectoryPropertiesFile file)
        {          
            // remove file from result
            return null;
        }

        IFile Convert(IDirectory newParent, IFile file)
        {
            // ignore file instances that are not instances of FilePropertiesFile or DirectoryPropertiesFile
            return null;
        }

        string GetDirectoryName(IFile file) => null;

        string GetDirectoryName(DirectoryPropertiesFile file) => file.Content.Name;

        dynamic ConvertDynamic(IDirectory newParent, dynamic toConvert) 
            => ((dynamic) this).Convert(newParent, toConvert);

        string GetDirectoryNameDynamic(dynamic file) => ((dynamic)this).GetDirectoryName(file);
    }
}