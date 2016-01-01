// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;

namespace SyncTool.Configuration.Model
{
    /// <summary>
    /// Configuration object for a folder that#s being synced within a group
    /// </summary>
    public class SyncFolder : IEquatable<SyncFolder>
    {
        /// <summary>
        /// Gets or sets the name of the folder
        /// </summary>
        public string Name { get; set; }
      
        /// <summary>
        /// Gets or sets the path of the folders root directory in the filesystem
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the filesystem filter for this folder
        /// </summary>
        public FileSystemFilterConfiguration Filter { get; set; }


        public override int GetHashCode() => StringComparer.InvariantCultureIgnoreCase.GetHashCode(this.Name);

        public override bool Equals(object obj) => Equals(obj as SyncFolder);

        public bool Equals(SyncFolder other)
        {
            if (other == null)
            {
                return false;                
            }

            var comparer = StringComparer.InvariantCultureIgnoreCase;

            return comparer.Equals(this.Name, other.Name) && comparer.Equals(this.Path, other.Path) &&
                   (Object.ReferenceEquals(this.Filter, other.Filter) || (this.Filter != null && this.Filter.Equals(other.Filter)));
        }
    }
}