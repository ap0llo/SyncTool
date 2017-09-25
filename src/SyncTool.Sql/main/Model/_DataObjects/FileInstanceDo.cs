using System;
using JetBrains.Annotations;
using NodaTime;

namespace SyncTool.Sql.Model
{
    public class FileInstanceDo
    {
        public int Id { get; set; }
        
        public FileDo File { get; set; }
                
        public long LastWriteUnixTimeTicks { get; set; }

        public long Length { get; set; }


        [UsedImplicitly]
        public FileInstanceDo()
        { }

        public FileInstanceDo(FileDo file, Instant lastWriteTime, long length)
        {
            File = file;                        
            LastWriteUnixTimeTicks = lastWriteTime.ToUnixTimeTicks();
            Length = length;
        }
    }
}
