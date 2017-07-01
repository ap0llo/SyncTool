using Microsoft.AspNetCore.Mvc;
using SyncTool.Common.Groups;
using SyncTool.WebUI.Model.Groups;

namespace SyncTool.WebUI.Controllers
{
    public class GroupsController : ControllerBase
    {

        public GroupsController(IGroupManager groupManager) : base(groupManager)
        {
        }


        public IActionResult Index([FromQuery] string groupName = null)
        {
            if(groupName == null)
            {                
                var model = new IndexModel()
                {
                    GroupNames = m_GroupManager.Groups
                };

                return View(model);
            }
            else
            {
                using (var group = m_GroupManager.OpenShared(groupName))
                {                    
                    var model = new DetailsModel()
                    {
                        GroupName = group.Name
                    };

                    return View("Details", model);
                }
            }
        }

      
    }
}
