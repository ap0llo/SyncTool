using System;
using JetBrains.Annotations;
using SyncTool.FileSystem;
using NodaTime;

namespace SyncTool.Sql.Model
{
    public sealed class FileReferenceDo
    {
        public int Id { get; set; }

        [CanBeNull]
        public string Path { get; set; }

        [CanBeNull]
        public long? LastWriteUnixTimeTicks { get; set; }

        [CanBeNull]
        public long? Length { get; set; }


        [UsedImplicitly]
        public FileReferenceDo()
        {
        }

        public FileReferenceDo(string path, long? lastWriteTimeUnixTicks, long? length)
        {
            Path = path;
            LastWriteUnixTimeTicks = lastWriteTimeUnixTicks;
            Length = length;
        }


        public FileReference ToFileReference()
        {
            Instant? lastWriteTime = default;
            if(LastWriteUnixTimeTicks.HasValue)
            {
                lastWriteTime = Instant.FromUnixTimeTicks(LastWriteUnixTimeTicks.Value);
            }

            return new FileReference(Path, lastWriteTime, Length);
        }

        public static FileReferenceDo FromFileReference(FileReference fileReference)
        {
            if (fileReference == null)
                throw new ArgumentNullException(nameof(fileReference));

            return new FileReferenceDo()
            {
                Id = 0,
                Path = fileReference.Path,
                LastWriteUnixTimeTicks = fileReference.LastWriteTime?.ToUnixTimeTicks(),
                Length = fileReference.Length
            };
        }
        
    }
}
