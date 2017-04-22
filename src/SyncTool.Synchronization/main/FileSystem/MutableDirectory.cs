using System;

namespace SyncTool.FileSystem
{
    /// <summary>
    /// Implementation of <see cref="IDirectory"/> that allows removal of directories
    /// </summary>
    internal class MutableDirectory : Directory
    {
        public MutableDirectory(string name) : base(name)
        {
        }

        public MutableDirectory(MutableDirectory parent, string name) : base(parent, name)
        {
            
        }

        public new void RemoveFileByName(string name) => base.RemoveFileByName(name);        

        public new MutableDirectory GetDirectory(string path) => (MutableDirectory)base.GetDirectory(path);

        public IDirectory Add(Func<MutableDirectory, MutableDirectory> createDirectory)
        {
            return base.Add(_ => createDirectory(this));
        }
    }
}