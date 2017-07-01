using Microsoft.AspNetCore.Mvc;
using SyncTool.Common.Groups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SyncTool.WebUI.Controllers
{
    public abstract class ControllerBase : Controller
    {
        protected readonly IGroupManager m_GroupManager;


        public ControllerBase(IGroupManager groupManager)
        {
            m_GroupManager = groupManager ?? throw new ArgumentNullException(nameof(groupManager));
        }
    }
}
