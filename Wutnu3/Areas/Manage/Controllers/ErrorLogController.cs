using System.Web.Mvc;
using Wutnu.Common;
using Wutnu.Data;
using Wutnu.Infrastructure;

namespace Wutnu.Areas.Manage.Controllers
{
    [Authorize(Roles = WutRoles.WutNuAdmins)]
    public class ErrorLogController : BaseController
    {
        public ErrorLogController(WutCache cache, WutNuContext models)
            :base(cache, models)
        {
        }

        public ActionResult Index()
        {
            ViewBag.SiteName = "Wut Admin";
            return View();
        }
    }
}
