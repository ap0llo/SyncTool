using SyncTool.FileSystem;
using System.Collections.Generic;

namespace SyncTool.Sql.Model
{
    public class DirectoryDo
    {
        public const string TableName = "Directories";

        public int Id { get; set; }

        public string NormalizedPath { get; set; }

        public string Name { get; set; }

        public List<DirectoryInstanceDo> Instances { get; set; } = new List<DirectoryInstanceDo>();

        
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
