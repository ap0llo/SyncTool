// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using System.IO;
using Newtonsoft.Json;

namespace SyncTool.FileSystem.Versioning.Git
{
    /// <summary>
    /// Class that provides a simple way to store properties of a <see cref="IFile"/>.    
    /// </summary>
    public class FileProperties : IEquatable<FileProperties>
    {

        /// <summary>
        /// The Name of the file
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The time the file was last modified
        /// </summary>
        public DateTime LastWriteTime { get; set; }

        /// <summary>
        /// The size of the file in bytes
        /// </summary>
        public long Length { get; set; }


        /// <summary>
        /// Initializes a new instance of <see cref="FileProperties"/> by copying all properties from the specified <see cref="IFile"/> instance
        /// </summary>
        public FileProperties(IFile file)
        {
            LastWriteTime = file.LastWriteTime;
            Length = file.Length;
            Name = file.Name;
        }

        /// <summary>
        /// Initializes a new (empty) instance of <see cref="FileProperties"/>
        /// </summary>
        public FileProperties()
        {
        }



        public override int GetHashCode() => StringComparer.InvariantCultureIgnoreCase.GetHashCode(this.Name);

        public override bool Equals(object obj) => Equals(obj as FileProperties);

        public bool Equals(FileProperties other)
        {
            if (other == null)
            {
                return false;
            }

            return StringComparer.InvariantCultureIgnoreCase.Equals(this.Name, other.Name) &&
                   this.LastWriteTime == other.LastWriteTime &&
                   this.Length == other.Length;
        }
    }
}