using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncTool.Sql.Model
{
    public class DirectoryInstanceDo
    {
        public int Id { get; set; }

        public DirectoryDo Directory { get; set; }
        
        public List<DirectoryInstanceDo> Directories { get; set; }

        public List<FileInstanceDo> Files { get; set; }
    }
}
