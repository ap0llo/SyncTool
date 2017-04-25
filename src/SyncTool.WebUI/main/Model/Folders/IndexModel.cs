using SyncTool.Configuration.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SyncTool.WebUI.Model.Folders
{
    public class IndexModel
    {
        public string GroupName { get; set; }

        public IEnumerable<SyncFolder> Folders { get; set; }

    }
}
