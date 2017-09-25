using System;

namespace SyncTool.FileSystem
{
    public abstract class FileSystemItem : IFileSystemItem
    {
        public virtual string Name { get;  }

        public string Path => Parent == null ? "" : Parent.Path + "/" + Name;


        public IDirectory Parent { get; }


        protected FileSystemItem(IDirectory parent, string name)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name must not be empty or whitespace", nameof(name));

            Parent = parent;
            Name = name;
        }
    }
}