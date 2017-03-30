// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using SyncTool.FileSystem;

namespace SyncTool.Git.FileSystem.Versioning.MetaFileSystem
{
    /// <summary>
    /// Represents a file containing a serialized <see cref="FileProperties"/> instance
    /// </summary>
    class FilePropertiesFile : DataFile<FileProperties>
    {
        /// <summary>
        /// The suffix used to identify file properties files 
        /// </summary>
        public const string FileNameSuffix = ".SyncToolFileInfo.json";
        

        private FilePropertiesFile(IDirectory parent, IFile file) : base(parent, file.Name + FileNameSuffix, new FileProperties(file))
        {                        
        }

        private FilePropertiesFile(IDirectory parent, string name, DateTime lastWriteTime, FileProperties content)
            : base(parent, name, content)
        {            
            LastWriteTime = lastWriteTime;
        }
        
        

        public override IFile WithParent(IDirectory newParent)
        {
            return new FilePropertiesFile(newParent, this.Name, this.LastWriteTime, this.Content);
        }     


        /// <summary>
        /// Creates a new <see cref="FilePropertiesFile"/> encapsulating the properties of the specified file instance
        /// </summary>
        public static FilePropertiesFile ForFile(IDirectory parent, IFile file) => new FilePropertiesFile(parent, file);

        /// <summary>
        /// Loads a <see cref="FilePropertiesFile"/> written out into a file 
        /// </summary>
        public static FilePropertiesFile Load(IDirectory parentDirectory, IReadableFile file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (!file.Name.EndsWith(FileNameSuffix, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ArgumentException($"File name Name must end with {FileNameSuffix}", nameof(file));
            }

            using (var stream = file.OpenRead())
            {
                return new FilePropertiesFile(parentDirectory, file.Name, file.LastWriteTime, stream.Deserialize<FileProperties>());
            }            
        }
    }
}