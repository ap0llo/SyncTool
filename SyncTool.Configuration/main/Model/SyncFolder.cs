// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Diagnostics;

namespace SyncTool.Configuration.Model
{
    public class SyncFolder : IEquatable<SyncFolder>
    {
        public string Name { get; set; }
      
        public string Path { get; set; }

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