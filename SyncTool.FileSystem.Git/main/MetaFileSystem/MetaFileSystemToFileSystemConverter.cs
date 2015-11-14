// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
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
        public IFileSystemMapping Convert(IDirectory directory)
        {
            var result = new FileSystemMapping();
            
            VisitDynamic(directory, new Stack<string>(), new List<IDirectory>(),  new List<IFile>(), result);

            return result;
        }


        void Visit(IDirectory directory, Stack<string> directoryNames, List<IDirectory> directoriesInParent, List<IFile> filesInParent, FileSystemMapping result)
        {
            // visit directories and files

            directoryNames.Push(directory.Name);

            var directories = new List<IDirectory>();
            var files = new List<IFile>();
            
            foreach (var subDir in directory.Directories)
            {
                VisitDynamic(subDir, directoryNames, directories, files, result);
            }

            foreach (var file in directory.Files)
            {
                VisitDynamic(file, directoryNames, directories,  files, result);
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
            result.AddMapping(directory, newDir);
        }

        void Visit(FilePropertiesFile file, Stack<string> directoryNames, List<IDirectory> directoriesInParent, List<IFile> filesInParent , FileSystemMapping result)
        {
            // load file properties
            filesInParent.Add(file.Content);
            result.AddMapping(file, file.Content);
        }

        void Visit(DirectoryPropertiesFile file, Stack<string> directoryNames, List<IDirectory> directoriesInParent, List<IFile> filesInParent, FileSystemMapping result)
        {
            // replace name of parent directory with the name from the properties file
            directoryNames.Pop();
            directoryNames.Push(file.Content.Name);
        }

        void Visit(IFile file, Stack<string> directoryNames, List<IDirectory> directories, List<IFile> files, FileSystemMapping result)
        {
            // ignore file instances that are not instances of FilePropertiesFile
        }

        void VisitDynamic(dynamic arg, Stack<string> directoryNames, List<IDirectory> directoriesInParent, List<IFile> filesInParent,  FileSystemMapping result)
        {
            ((dynamic) this).Visit(arg, directoryNames, directoriesInParent, filesInParent, result);
        }

    }
}