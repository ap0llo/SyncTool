// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using SyncTool.FileSystem.Git;
using SyncTool.FileSystem.Git.Utilities;

namespace SyncTool.FileSystem.Versioning.Git.MetaFileSystem
{
    /// <summary>
    ///  Converter that replaces all file instances in a directory with instances of <see cref="FilePropertiesFile"/> when appropriate
    /// </summary>
    public class MetaFileSystemLoader : FileSystemConverter
    {


        public override IDirectory Convert(IDirectory newParent, IDirectory toConvert)
        {
            var newDirectory = new Directory(newParent, toConvert.Name);

            foreach (var subDirectory in toConvert.Directories)
            {
                var newSubDirectory = (IDirectory) ConvertDynamic(newDirectory, subDirectory);
                if (newSubDirectory != null)
                {
                    newDirectory.Add(_ => newSubDirectory);
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

        public override IFile Convert(IDirectory newParent, IFile toConvert)
        {
            return toConvert.WithParent(newParent);
        }

        public IFile Convert(IDirectory newParent, IReadableFile file)
        {
            // replace all files which's name ends with .SyncTool.json with a FilePropertiesFile
            if (file.Name.EndsWith(FilePropertiesFile.FileNameSuffix, StringComparison.InvariantCultureIgnoreCase))
            {
                return FilePropertiesFile.Load(newParent, file);                
            }
            else if (file.Name.Equals(DirectoryPropertiesFile.FileName, StringComparison.InvariantCultureIgnoreCase))
            {
                return DirectoryPropertiesFile.Load(newParent, file);
            }
            else
            {
                // leave other files unchanged
                return file.WithParent(newParent);
            }
        }
    }
}