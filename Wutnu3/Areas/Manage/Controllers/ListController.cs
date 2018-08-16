using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wutnu.Data;
using Wutnu.Infrastructure;

namespace Wutnu.Areas.Manage.Controllers
{
    public class ListController : BaseController
    {
        public ListController(WutCache cache, WutNuContext models)
            :base(cache, models)
        {
        }

        // GET: List
        [Authorize]
        public ActionResult Index()
        {
            //todo: setup to let the user review all the links they've created before
            return View();
        }
    }
}