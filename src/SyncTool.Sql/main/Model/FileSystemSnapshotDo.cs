using System;
using System.Collections.Generic;

namespace SyncTool.Sql.Model
{
    public class FileSystemSnapshotDo
    {
        public FileSystemHistoryDo History { get; set; }

        public int Id { get; set; }
        
        public DateTime CreationTimeUtc { get; set; }

        public DirectoryInstanceDo RootDirectory { get; set; }

        public List<FileInstanceDo> IncludedFiles { get; set; }
    }
}
