using System;

namespace SyncTool.Sql.Model
{
    public class FileInstanceDo
    {
        public int Id { get; set; }

        public FileDo File { get; set; }

        public DateTime LastWriteTimeUtc { get; set; }

        public long Length { get; set; }        
    }
}
