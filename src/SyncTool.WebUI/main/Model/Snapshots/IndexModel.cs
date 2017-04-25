using SyncTool.FileSystem.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SyncTool.WebUI.Model.Snapshots
{
    
    public class IndexModel
    {
        public string GroupName { get; set; }

        public string FolderName { get; set; }

        public IEnumerable<IFileSystemSnapshot> Snapshots { get; set; }         
    }
}
