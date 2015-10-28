using System;
using System.Collections.Generic;
using System.Linq;

namespace SyncTool.FileSystem.Git
{
    public class MetaFileSystemCreator
    {


        public IDirectory CreateMetaDirectory(IDirectory directory)
        {
            var newDirectory = new Directory(
                directory.Name, 
                directory.Directories.Select(CreateMetaDirectory),
                directory.Files.Select(CreateMetaFile));        
            
            newDirectory.Add(DirectoryPropertiesFile.ForDirectory(directory));
            
            return newDirectory;
        }



        IFile CreateMetaFile(IFile file) => FilePropertiesFile.ForFile(file);



    }
}