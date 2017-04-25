using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SyncTool.Common;
using Microsoft.AspNetCore.Mvc;
using SyncTool.FileSystem.Versioning;
using SyncTool.WebUI.Model.Snapshots;

namespace SyncTool.WebUI.Controllers
{
    public class SnapshotsController : ControllerBase
    {
        public SnapshotsController(IGroupManager groupManager) : base(groupManager)
        {
        }

        public IActionResult Index([FromQuery] string groupName, [FromQuery] string folderName)
        {
            var group = m_GroupManager.GetGroup(groupName);
            var historyService = group.GetHistoryService();
            var history = historyService[folderName];

            var model = new IndexModel()
            {
                GroupName = group.Name,
                FolderName = history.Name,
                Snapshots = history.Snapshots
            };

            return View(model);        
        }


        public IActionResult Directory(
            string id, 
            [FromQuery] string groupName, 
            [FromQuery] string folderName,
            [FromQuery] string path)
        {
            var group = m_GroupManager.GetGroup(groupName);
            var historyService = group.GetHistoryService();
            var history = historyService[folderName];

            var snapshot = history[id];
            
            var model = new DirectoryModel()
            {
                GroupName = group.Name,
                FolderName = history.Name,
                SnapshotId = snapshot.Id,
                Directory = path == null ? snapshot.RootDirectory : snapshot.RootDirectory.GetDirectory(path)
            };

            return View(model);
        }


        
    }
}
