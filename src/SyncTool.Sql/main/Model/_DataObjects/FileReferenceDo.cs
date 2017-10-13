using System;
using JetBrains.Annotations;
using SyncTool.FileSystem;

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


        public static FileReferenceDo FromFileReference(IFileReference fileReference)
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
