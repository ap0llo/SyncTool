using System;

namespace SyncTool.Sql.Model
{
    public class FileInstanceDo
    {
        public int Id { get; set; }

        public FileDo File { get; set; }

        public DateTime LastWriteTimeUtc { get; set; }

        public long Length { get; set; }


        public FileInstanceDo()
        {
        }

        public FileInstanceDo(FileDo file, DateTime lastWriteTime, long length)
        {
            File = file;
            
            //TODO: special-casing DateTime.MinValue seems like a stupid hack, find a way to avoid ths
            LastWriteTimeUtc = lastWriteTime == DateTime.MinValue 
                ? DateTime.MinValue 
                : lastWriteTime.ToUniversalTime();

            Length = length;
        }
    }
}
