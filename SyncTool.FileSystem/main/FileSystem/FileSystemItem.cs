// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;

namespace SyncTool.FileSystem
{
    public abstract class FileSystemItem : IFileSystemItem
    {
        public virtual string Name { get;  }

        public string Path
        {
            get
            {                
                if (Parent == null)
                {
                    return "";
                }
                else
                {
                    var parentPath = Parent.Path;
                    if (String.IsNullOrEmpty(parentPath))
                    {
                        return Name;
                    }
                    else
                    {
                        return parentPath + "/" + Name;                        
                    }
                }
            }
        }

        public IDirectory Parent { get; }


        protected FileSystemItem(IDirectory parent, string name)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name must not be empty or whitespace", nameof(name));
            }

            Parent = parent;
            Name = name;
        }

    }
}