using System;
using System.Collections.Generic;
using System.Linq;

namespace SyncTool.FileSystem.Git
{
    /// <summary>    
    /// Converter that takes a "meta file system" and retrieves the original file system stored in there 
    /// </summary>
    internal class MetaFileSystemToFileSystemConverter
    {
        public IDirectory Convert(IDirectory directory)
        {
            var directories = new List<IDirectory>();

            VisitDynamic(directory, new Stack<string>(), directories,  new List<IFile>());
            return directories.Single();
        }


        void Visit(IDirectory directory, Stack<string> directoryNames, List<IDirectory> directoriesInParent,  List<IFile> filesInParent)
        {
            // visit directories and files

            directoryNames.Push(directory.Name);

            var directories = new List<IDirectory>();
            var files = new List<IFile>();
            
            foreach (var subDir in directory.Directories)
            {
                VisitDynamic(subDir, directoryNames, directories, files);
            }

            foreach (var file in directory.Files)
            {
                VisitDynamic(file, directoryNames, directories,  files);
            }

            // create a new directory to replace the current one
            var newDir = new Directory(directoryNames.Pop());

            // remove subdirectories from the stack
            foreach (var dir in directories)
            {
                newDir.Add(dir);
            }

            // get all files from the stack
            foreach (var file in files)
            {
                newDir.Add(file);
            }

            // push new directory to the stack
            directoriesInParent.Add(newDir);
        }

        void Visit(FilePropertiesFile file, Stack<string> directoryNames, List<IDirectory> directoriesInParent, List<IFile> filesInParent)
        {
            // load file properties
            var describedFile = new File(file.Content.Name)
            {
                LastWriteTime = file.Content.LastWriteTime,
                Length = file.Content.Length
            };

            filesInParent.Add(describedFile);
        }

        void Visit(DirectoryPropertiesFile file, Stack<string> directoryNames, List<IDirectory> directoriesInParent, List<IFile> filesInParent)
        {
            // replace name of parent directory with the name from the properties file
            directoryNames.Pop();
            directoryNames.Push(file.Content.Name);
        }



        void Visit(IFile file, Stack<string> directoryNames, List<IDirectory> directories, List<IFile> files)
        {
            // ignore file instances that are not instances of FilePropertiesFile
        }

        void VisitDynamic(dynamic arg, Stack<string> directoryNames, List<IDirectory> directoriesInParent, List<IFile> filesInParent)
        {
            ((dynamic) this).Visit(arg, directoryNames, directoriesInParent, filesInParent);
        }

    }
}