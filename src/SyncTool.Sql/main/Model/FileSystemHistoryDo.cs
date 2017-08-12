using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncTool.Sql.Model
{
    public class FileSystemHistoryDo
    {
        public const string TableName = "FileSystemHistories";


        public int Id { get; set; }
        
        public string Name { get; set; }

        public string NormalizedName { get; set; }
        
        public int Version { get; set; }


        public FileSystemHistoryDo()
        {
        }

        public FileSystemHistoryDo(string name)
        {
            Name = name;
            NormalizedName = name.NormalizeCaseInvariant();
        }
    }
}
