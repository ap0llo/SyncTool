using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SyncTool.WebUI.Controllers
{
    public class SnapshotsController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }
    }
}
