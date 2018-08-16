using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wutnu.Data;
using Wutnu.Infrastructure;
using Wutnu.Web.Infrastructure;

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
        public void GetCopyBlobZip()
        {
            var path = Request.MapPath("/Files/BlobCopy/");
            //Convert the memorystream to an array of bytes.
            var me = Wutcontext.Users.Single(u => u.UserId == UserId);
            var ApiUrl = string.Format("https://{0}/api/CopyUtil/", Request.Url.Authority);
            var zip = BlobCopyZip.SetupZip(path, me.ApiKey, ApiUrl);

            // Clear all content output from the buffer stream
            Response.Clear();
            // Add a HTTP header to the output stream that specifies the default filename
            // for the browser's download dialog
            Response.AddHeader("Content-Disposition", "attachment; filename=BlobCopy.zip");
            // Add a HTTP header to the output stream that contains the 
            // content length(File Size). This lets the browser know how much data is being transfered
            Response.AddHeader("Content-Length", zip.Length.ToString());
            // Set the HTTP MIME type of the output stream
            Response.ContentType = "application/octet-stream";
            // Write the data out to the client.
            Response.BinaryWrite(zip.ToArray());
        }
    }
}