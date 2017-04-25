using SyncTool.Configuration.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SyncTool.WebUI.Model.Folders
{
    public class DetailsModel
    {
        public string GroupName { get; set; }

        public SyncFolder Folder { get; set; }
    }
}
