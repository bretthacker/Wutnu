using Wutnu.Common;
using System;
using System.IdentityModel.Claims;
using System.Web.Mvc;
using Wutnu.Data;
using Infrastructure;
using Wutnu.Infrastructure;

namespace Wutnu.Areas.Manage.Controllers
{
    public class GenController : BaseController
    {
        public GenController(WutCache cache, WutNuContext models)
            :base(cache, models)
        {
        }

        // GET: Gen
        [HttpPost]
        public ActionResult Index(WutLink oUrl)
        {
            //create the record
            //var oUrl = new WutLink
            //{
            //    RealUrl = url,
            //    CreateDate = DateTime.UtcNow,
            //    CreatedByIp = Request.UserHostAddress,
            //    UseDelay = useDelay
            //};
            oUrl.CreateDate = DateTime.UtcNow;
            oUrl.CreatedByIp = Request.UserHostAddress;

            //tag it if logged in, for posterity or cool reporting later
            if (User.Identity.IsAuthenticated)
            {
                oUrl.UserId = User.Identity.GetClaim<int>(CustomClaimTypes.UserId);
            }

            //save it
            oUrl = ShortUrlUtils.AddUrlToDatabase(oUrl, Wutcontext);

            //set it up to display
            ViewBag.ShortUrl = ShortUrlUtils.PublicShortUrl(oUrl.ShortUrl);
            ViewBag.NavigateUrl = oUrl.RealUrl;

            //go back home
            return View("~/Views/Home/Index.cshtml");
        }
    }
}
