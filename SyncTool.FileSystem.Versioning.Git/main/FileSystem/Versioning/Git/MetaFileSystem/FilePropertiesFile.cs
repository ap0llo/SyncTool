// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.IO;

namespace SyncTool.FileSystem.Versioning.Git.MetaFileSystem
{
    /// <summary>
    /// Represents a file containing a serialized <see cref="FileProperties"/> instance
    /// </summary>
    class FilePropertiesFile : FileSystemItem, IReadableFile
    {
        /// <summary>
        /// The suffix used to identify file properties files 
        /// </summary>
        public const string FileNameSuffix = ".SyncToolFileInfo.json";
        

        private FilePropertiesFile(IDirectory parent, IFile file) : base(parent, file.Name + FileNameSuffix)
        {            
            Content = new FileProperties(file);
            LastWriteTime = DateTime.Now;
        }

        private FilePropertiesFile(IDirectory parent, string name, DateTime lastWriteTime, FileProperties content)
            : base(parent, name)
        {
            Content = content;
            LastWriteTime = lastWriteTime;
        }
        
        
        public DateTime LastWriteTime { get; }

        public long Length { get { throw new NotSupportedException();} }

        public IFile WithParent(IDirectory newParent)
        {
            return new FilePropertiesFile(newParent, this.Name, this.LastWriteTime, this.Content);
        }

        /// <summary>
        /// Gets the <see cref="FileProperties"/> this file contains
        /// </summary>
        public FileProperties Content { get; }

        public Stream OpenRead()
        {            
            using (var writeStream = new MemoryStream())            
            {
                Content.WriteTo(writeStream);
                writeStream.Flush();

                return new MemoryStream(writeStream.ToArray());
            }            
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