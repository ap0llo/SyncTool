// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace SyncTool.FileSystem.Filtering
{
    public class FilteringConverter
    {
        readonly IFileSystemFilter m_Filter;

        public FilteringConverter(IFileSystemFilter filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }
            m_Filter = filter;
        }



        public IDirectory Convert(IDirectory directory) => Convert(null, directory);



        IDirectory Convert(IDirectory parentDirectory, IDirectory toConvert)
        {
            var newDirectory = new Directory(null, toConvert.Name);

            foreach (var subDirectory in toConvert.Directories)
            {
                if (m_Filter.Applies(subDirectory) == false)
                {
                    newDirectory.Add(d => Convert(d, subDirectory));                    
                }
            }

            foreach (var file in toConvert.Files)
            {
                if (m_Filter.Applies(file) == false)
                {
                    newDirectory.Add(d => file.WithParent(d));
                }
            }

            return newDirectory;
        }

    }
}