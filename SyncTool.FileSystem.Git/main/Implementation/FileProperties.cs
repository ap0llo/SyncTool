using System;

namespace SyncTool.FileSystem.Git
{
        public class FileProperties
        {
            public DateTime LastWriteTime { get; set; }

            public long Length { get; set; }

            public FileProperties(IFile file)
            {

                LastWriteTime = file.LastWriteTime;
                Length = file.Length;
            }

            public FileProperties()
            {                
            }
        }
}