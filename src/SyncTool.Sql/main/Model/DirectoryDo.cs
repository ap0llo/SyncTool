using SyncTool.FileSystem;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace SyncTool.Sql.Model
{
    public class DirectoryDo
    {
        
        public int Id { get; set; }

        public string NormalizedPath { get; set; }

        public string Name { get; set; }

        public List<DirectoryInstanceDo> Instances { get; set; } = new List<DirectoryInstanceDo>();

        
        [UsedImplicitly]
        public DirectoryDo()
        {
        }
        
        public static DirectoryDo FromDirectory(IDirectory directory)
        {
            return new DirectoryDo()
            {
                Name = directory.Name,
                NormalizedPath = directory.Path.NormalizeCaseInvariant(),
            };
        }
    }
}
