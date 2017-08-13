using System;

namespace SyncTool.Sql.Model
{
    public class FileInstanceDo
    {
        public int Id { get; set; }
        
        public FileDo File { get; set; }
                
        public long LastWriteTimeTicks { get; set; }

        public long Length { get; set; }


        public FileInstanceDo()
        {
        }

        public FileInstanceDo(FileDo file, DateTime lastWriteTime, long length)
        {
            File = file;                        
            LastWriteTimeTicks = lastWriteTime.Ticks;
            Length = length;
        }
    }
}
