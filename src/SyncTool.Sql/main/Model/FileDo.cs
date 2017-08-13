using SyncTool.FileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncTool.Sql.Model
{
    public class FileDo
    {
        

        public int Id { get; set; }

        public string NormalizedPath { get; set; }

        public string Name { get; set; }

        public List<FileInstanceDo> Instances { get; set; }


        public FileDo()
        {
        }

        public static FileDo FromFile(IFile file)
        {
            return new FileDo()
            {
                Name = file.Name,
                NormalizedPath = file.Path.NormalizeCaseInvariant(),
                Instances = new List<FileInstanceDo>()
            };
        }
    }
}
