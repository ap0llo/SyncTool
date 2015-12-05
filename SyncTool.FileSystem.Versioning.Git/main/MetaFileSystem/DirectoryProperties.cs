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
    /// Serializable properties of a directory
    /// </summary>
    public class DirectoryProperties : IEquatable<DirectoryProperties>
    {

        /// <summary>
        /// The name of the directory
        /// </summary>
        public string Name { get; set; }


        /// <summary>
        /// Initializes a new instance of <see cref="DirectoryProperties"/> by copying all properties from the specified <see cref="IDirectory"/> instance
        /// </summary>
        public DirectoryProperties(IDirectory directory)
        {
            if (directory == null)
            {
                throw new ArgumentNullException(nameof(directory));
            }

            this.Name = directory.Name;
        }

        /// <summary>
        /// Initializes a new (empty) instance of <see cref="DirectoryProperties"/>
        /// </summary>
        public DirectoryProperties()
        {
            
        }


        public override int GetHashCode() => StringComparer.InvariantCultureIgnoreCase.GetHashCode(this.Name);

        public override bool Equals(object obj) => Equals(obj as DirectoryProperties);

        public bool Equals(DirectoryProperties other)
        {
            if (other == null)
            {
                return false;
            }

            return StringComparer.InvariantCultureIgnoreCase.Equals(this.Name, other.Name);
        }
    }
}