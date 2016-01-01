// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

namespace SyncTool.Configuration.Model
{
    /// <summary>
    /// Configuration object for a folder's filter
    /// </summary>
    public class FileSystemFilterConfiguration
    {
         /// <summary>
         /// Gets or sets the Type of the filter
         /// </summary>
        public FileSystemFilterType Type { get; set; }

        /// <summary>
        /// Gets or sets the filter's query string. 
        /// The query string is interpreted based on the filter's type
        /// </summary>
        public string Query { get; set; }

    }
}