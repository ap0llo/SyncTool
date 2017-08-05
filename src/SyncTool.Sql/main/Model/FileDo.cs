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
    }
}
