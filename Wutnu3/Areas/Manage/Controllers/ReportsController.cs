using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wutnu.Common;
using Wutnu.Data;
using Wutnu.Infrastructure;
using Wutnu.Web.Infrastructure;

namespace Wutnu.Areas.Manage.Controllers
{
    [Authorize]
    public class ReportsController : BaseController
    {
        public ReportsController(WutCache cache, WutNuContext models)
            :base(cache, models)
        {
        }

        public ActionResult Run()
        {
            // return to page
            return View("/Blob/Report/" + Request.Url.Segments.Last());
        }
    }
}
