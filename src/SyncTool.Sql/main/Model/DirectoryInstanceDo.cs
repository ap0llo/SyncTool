using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace SyncTool.Sql.Model
{
    public class DirectoryInstanceDo
    {
        public int Id { get; set; }

        public DirectoryDo Directory { get; set; }
        
        public List<DirectoryInstanceDo> Directories { get; set; }

        public List<FileInstanceDo> Files { get; set; }


        [UsedImplicitly]
        public DirectoryInstanceDo()
        {
        }

        public DirectoryInstanceDo(DirectoryDo directoryDo)
        {
            Directory = directoryDo ?? throw new ArgumentNullException(nameof(directoryDo));
            Directories = new List<DirectoryInstanceDo>();
            Files = new List<FileInstanceDo>();
        }
    }
}
