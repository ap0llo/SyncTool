// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using SyncTool.FileSystem.Git;

namespace SyncTool.FileSystem.Versioning.Git
{
    /// <summary>    
    /// Converter that takes a "meta file system" and retrieves the original file system stored in there 
    /// </summary>
    internal class MetaFileSystemToFileSystemConverter
    {
        public IFileSystemMapping Convert(IDirectory directory)
        {
            var result = new FileSystemMapping();
            
            ConvertDynamic(null, directory, result);

            return result;
        }



        IDirectory Convert(IDirectory newParent, IDirectory toConvert, FileSystemMapping mapping)
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
            mapping.AddMapping(toConvert, newDirectory);

            foreach (var childDirectory in toConvert.Directories)
            {
                var newChildDirectory = (IDirectory) ConvertDynamic(newDirectory, childDirectory, mapping);
                if (newChildDirectory != null)
                {
                    newDirectory.Add(_ => newChildDirectory);
                }

                
            }

            foreach (var file in toConvert.Files)
            {
                var newFile = (IFile) ConvertDynamic(newDirectory, file, mapping);
                if (newFile != null)
                {
                    newDirectory.Add(_ => newFile);
                }
            }


            return newDirectory;
        }

        IFile Convert(IDirectory newParent, FilePropertiesFile file,  FileSystemMapping mapping)
        {
            // load file properties            
            var newFile = new File(newParent, file.Content.Name) { LastWriteTime =  file.Content.LastWriteTime, Length = file.Content.Length };
            mapping.AddMapping(file, newFile);

            return newFile;
        }

        IFile Convert(IDirectory newParent, DirectoryPropertiesFile file, FileSystemMapping mapping)
        {          
            // remove file from result
            //mapping.AddMapping(file, null);
            return null;
        }

        IFile Convert(IDirectory newParent, IFile file, FileSystemMapping mapping)
        {
            // ignore file instances that are not instances of FilePropertiesFile or DirectoryPropertiesFile
            //mapping.AddMapping(file, null);
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

        dynamic ConvertDynamic(IDirectory newParent, dynamic toConvert, FileSystemMapping mapping)
        {
            return ((dynamic) this).Convert(newParent, toConvert, mapping);
        }


        string GetDirectoryNameDynamic(dynamic file)
        {
            return ((dynamic)this).GetDirectoryName(file);
        }
    }
}