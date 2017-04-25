using SyncTool.FileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SyncTool.WebUI.Model.Snapshots
{
    public class DirectoryModel
    {
        public string GroupName { get; set; }

        public string FolderName { get; set; }

        public string SnapshotId { get; set; }

        public IDirectory Directory { get; set; }        
    }
}
