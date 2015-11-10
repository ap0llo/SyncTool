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
    class FilePropertiesFile : IReadableFile
    {
        /// <summary>
        /// The suffix used to identify file properties files 
        /// </summary>
        public const string FileNameSuffix = ".SyncToolFileInfo.json";
        

        private FilePropertiesFile(IFile file)
        {
            Name = file.Name + FileNameSuffix;
            Content = new FileProperties(file);
            LastWriteTime = DateTime.Now;
        }

        private FilePropertiesFile(string name, DateTime lastWriteTime, FileProperties content)
        {
            Name = name;
            Content = content;
            LastWriteTime = lastWriteTime;
        }

                
        public string Name { get;  }
        
        public DateTime LastWriteTime { get; }

        public long Length { get { throw new NotSupportedException();} }

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
        public static FilePropertiesFile ForFile(IFile file) => new FilePropertiesFile(file);

        /// <summary>
        /// Loads a <see cref="FilePropertiesFile"/> written out into a file 
        /// </summary>
        public static FilePropertiesFile Load(IReadableFile file)
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
                return new FilePropertiesFile(file.Name, file.LastWriteTime, stream.Deserialize<FileProperties>());
            }            
        }
    }
}