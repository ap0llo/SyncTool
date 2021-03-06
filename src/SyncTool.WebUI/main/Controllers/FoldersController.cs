﻿using Microsoft.AspNetCore.Mvc;
using SyncTool.Common.Groups;
using SyncTool.Configuration;
using SyncTool.WebUI.Model.Folders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SyncTool.WebUI.Controllers
{
    public class FoldersController : ControllerBase
    {
        public FoldersController(IGroupManager groupManager) : base(groupManager)
        {
        }

        public IActionResult Index([FromQuery] string groupName, [FromQuery] string folderName)
        {
            using(var group = m_GroupManager.OpenShared(groupName))
            {
                var configService = group.GetConfigurationService();

                if (String.IsNullOrEmpty(folderName))
                {
                    var folders = configService.Items;
                    var model = new IndexModel()
                    {
                        GroupName = group.Name,
                        Folders = folders
                    };

                    return View(model);
                }
                else
                {
                    var folder = configService[folderName];

                    var model = new DetailsModel()
                    {
                        GroupName = groupName,
                        Folder = folder
                    };

                    return View("Details", model);
                }
            }
            
        }

    }
}
