﻿// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using SyncTool.FileSystem;

namespace SyncTool.Git.FileSystem.Versioning.MetaFileSystem
{
    /// <summary>    
    /// Converter that takes a "meta file system" and retrieves the original file system stored in there 
    /// </summary>
    internal class MetaFileSystemToFileSystemConverter
    {
        public IDirectory Convert(IDirectory directory)
        {
            return ConvertDynamic(null, directory);         
        }



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


        string GetDirectoryName(IFile file)
        {
            return null;
        }

        string GetDirectoryName(DirectoryPropertiesFile file)
        {
            return file.Content.Name;
        }

        dynamic ConvertDynamic(IDirectory newParent, dynamic toConvert)
        {
            return ((dynamic) this).Convert(newParent, toConvert);
        }


        string GetDirectoryNameDynamic(dynamic file)
        {
            return ((dynamic)this).GetDirectoryName(file);
        }
    }
}