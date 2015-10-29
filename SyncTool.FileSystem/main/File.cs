using System;

namespace SyncTool.FileSystem
{
    public class File : IFile
    {
        
        public string Name { get; set; }

        public DateTime LastWriteTime { get; set; }

        public long Length { get; set; }        

        public File()
        {
            
        }

        public File(string name)
        {
            this.Name = name;
        }

    }
}