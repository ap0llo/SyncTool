// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015-2016, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;

namespace SyncTool.Configuration.Model
{
    /// <summary>
    /// Configuration object for a folder's filter
    /// </summary>
    public sealed class FilterConfiguration : IEquatable<FilterConfiguration>
    {
        public static readonly FilterConfiguration Empty = new FilterConfiguration(FilterType.None,null);

         /// <summary>
         /// Gets the Type of the filter
         /// </summary>
        public FilterType Type { get;  }

        /// <summary>
        /// Gets the filter's settings string. 
        /// The semantics of this property depends on the filter type
        /// </summary>
        public string CustomData { get; }


        public FilterConfiguration(FilterType type, string customData)
        {
            Type = type;
            CustomData = customData;
        }


        public override bool Equals(object obj) => Equals(obj as FilterConfiguration);

        public override int GetHashCode()
        {            
            return ((int) Type*397) ^ (CustomData != null ? CustomData.GetHashCode() : 0);           
        }
        
        public bool Equals(FilterConfiguration other)
        {
            if (other == null)
            {
                return false;
            }
            else
            {
                return other.Type == this.Type && other.CustomData == this.CustomData;
            }
        }
    }
}