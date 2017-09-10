using SyncTool.FileSystem;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace SyncTool.Sql.Model
{
    public class FileDo
    {
        public int Id { get; set; }

        public string NormalizedPath { get; set; }

        public string Path { get; set; }

        public string Name { get; set; }

        public List<FileInstanceDo> Instances { get; set; }

        [UsedImplicitly]
        public FileDo()
        {
        }

        public static FileDo FromFile(IFile file)
        {
            return new FileDo()
            {
                Name = file.Name,
                Path = file.Path,
                NormalizedPath = file.Path.NormalizeCaseInvariant(),
                Instances = new List<FileInstanceDo>()
            };
        }
    }
}
