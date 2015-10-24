using System;

namespace SyncTool.FileSystem
{
    public abstract class File : IFile
    {
        
        public string Name { get; set; }

        public DateTime LastWriteTime { get; set; }

        public abstract long Length { get; set; }        

        protected File()
        {
            
        }

        protected File(string name)
        {
            this.Name = name;
        }

    }
}