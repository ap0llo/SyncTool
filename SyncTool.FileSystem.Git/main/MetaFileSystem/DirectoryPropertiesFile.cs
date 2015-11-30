// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using System.IO;

namespace SyncTool.FileSystem.Git
{
    /// <summary>
    /// Represents a file containing a serialized <see cref="FileProperties"/> instance
    /// </summary>
    public class DirectoryPropertiesFile : FileSystemItem, IReadableFile
    {

        /// <summary>
        /// The name of the file
        /// </summary>
        public const string FileName = "SyncToolDirectoryInfo.json";


        private DirectoryPropertiesFile(IDirectory parentDirectory, IDirectory directory) : base(parentDirectory, FileName)
        {            
            Content = new DirectoryProperties(directory);
            LastWriteTime = DateTime.Now;
        }

        // constructor needs to be internal because it is used by tests
        internal DirectoryPropertiesFile(IDirectory parentDirectory, DateTime lastWriteTime, DirectoryProperties content) : base(parentDirectory, FileName)
        {            
            Content = content;
            LastWriteTime = lastWriteTime;
        }
        

        public DateTime LastWriteTime { get; }

        public long Length { get { throw new NotSupportedException(); } }

        public IFile WithParent(IDirectory newParent)
        {
            return new DirectoryPropertiesFile(newParent, this.LastWriteTime, this.Content);
        }

        /// <summary>
        /// Gets the <see cref="DirectoryProperties"/> this file contains
        /// </summary>
        public DirectoryProperties Content { get; }

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
        /// Creates a new <see cref="DirectoryPropertiesFile"/> encapsulating the properties of the specified directory instance
        /// </summary>
        public static DirectoryPropertiesFile ForDirectory(IDirectory parentDirectory, IDirectory directory) => new DirectoryPropertiesFile(parentDirectory, directory);

        /// <summary>
        /// Loads a <see cref="DirectoryPropertiesFile"/> written out into a file 
        /// </summary>
        public static DirectoryPropertiesFile Load(IDirectory parentDirectory, IReadableFile file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (!file.Name.Equals(FileName, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ArgumentException($"File name has to be {FileName}", nameof(file));
            }

            using (var stream = file.OpenRead())
            {
                return new DirectoryPropertiesFile(parentDirectory, file.LastWriteTime, stream.Deserialize<DirectoryProperties>());
            }
        }

    }
}