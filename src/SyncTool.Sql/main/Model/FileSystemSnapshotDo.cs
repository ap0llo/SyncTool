using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace SyncTool.Sql.Model
{
    public class FileSystemSnapshotDo
    {
        public int HistoryId { get; set; }
        
        // assigned automatically on db insert
        public int Id { get; set; }
        
        // assigned automatically on db insert
        public int SequenceNumber { get; set; }
        
        public long CreationTimeTicks { get; set; }

        public int RootDirectoryInstanceId { get; set; }

        public List<FileInstanceDo> IncludedFiles { get; set; } = new List<FileInstanceDo>();


        [UsedImplicitly]
        public FileSystemSnapshotDo()
        {
        }

        public FileSystemSnapshotDo(
            int historyId, 
            long creationTimeTicks, 
            int rootDirectoryInstanceId,
            List<FileInstanceDo> includedFiles)
        {
            if (rootDirectoryInstanceId <= 0)
                throw new ArgumentOutOfRangeException(nameof(rootDirectoryInstanceId));

            HistoryId = historyId;
            CreationTimeTicks = creationTimeTicks;
            RootDirectoryInstanceId = rootDirectoryInstanceId;
            IncludedFiles = includedFiles;            
        }
    }
}
