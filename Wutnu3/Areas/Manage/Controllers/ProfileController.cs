using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wutnu.Common;
using Wutnu.Data;
using Wutnu.Data.Models;
using Wutnu.Infrastructure;

namespace Wutnu.Areas.Manage.Controllers
{
    [Authorize]
    public class ProfileController : BaseController
    {
        public ProfileController(WutCache cache, WutNuContext models)
            :base(cache, models)
        {
        }

        // GET: Manage/Profile
        public ActionResult Index()
        {
            UserPoco res = UserPoco.UserToUserPoco(Wutcontext.Users.Single(u => u.UserId == UserId));
            return View(res);
        }
    }
}