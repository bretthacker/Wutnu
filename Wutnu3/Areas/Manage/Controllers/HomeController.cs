using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wutnu.Data;
using Wutnu.Infrastructure;

namespace Wutnu.Areas.Manage.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        public HomeController(WutCache cache, WutNuContext models)
            :base(cache, models)
        {
        }

        // GET: Manage/Home
        public ActionResult Index()
        {
            return View();
        }
    }
}