using System;
using System.Collections.Generic;

namespace SyncTool.Sql.Model
{
    public class DirectoryInstanceDo
    {
        public int Id { get; set; }

        public DirectoryDo Directory { get; set; }

        public List<DirectoryInstanceDo> Directories { get; set; } = new List<DirectoryInstanceDo>();

        public List<FileInstanceDo> Files { get; set; } = new List<FileInstanceDo>();


        public DirectoryInstanceDo()
        {
        }

        public DirectoryInstanceDo(DirectoryDo directoryDo)
        {
            Directory = directoryDo ?? throw new ArgumentNullException(nameof(directoryDo));
        }
    }
}
