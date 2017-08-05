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

        public List<FileInstanceDo> IncludedFiles { get; set; } = new List<FileInstanceDo>();


        public FileSystemSnapshotDo()
        {
        }

        public FileSystemSnapshotDo(
            FileSystemHistoryDo history, 
            DateTime creationTimeUtc, 
            DirectoryInstanceDo rootDirectory,
            List<FileInstanceDo> includedFiles)
        {
            History = history;
            CreationTimeUtc = creationTimeUtc;
            RootDirectory = rootDirectory;
            IncludedFiles = includedFiles;
        }
    }
}
