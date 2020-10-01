﻿using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wutnu.Common;
using Wutnu.Common.ErrorMgr;
using Wutnu.Data;
using Wutnu.Repo;
using Wutnu.Models;
using Newtonsoft.Json;
using System.IdentityModel.Claims;
using Wutnu.Infrastructure;

namespace Wutnu.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(WutCache cache, WutNuContext models)
            :base(cache, models)
        {
        }

        public ActionResult Index()
        {
            if (Request.Cookies["Error"] != null)
            {
                ViewBag.Error = Request.Cookies["Error"].Value;
            }
            return CheckForReturn();
        }

        private ActionResult CheckForReturn()
        {
            if (Request.UrlReferrer == null)
                return View();

            var profile = HttpUtility.ParseQueryString(Request.UrlReferrer.Query)["p"];
            if (profile != null && Request.UrlReferrer.AbsolutePath.Split('/').Last()!="logout")
            {
                profile = profile.ToLower();

                if (profile==Startup.ResetPolicyId.ToLower() || profile == Startup.ProfilePolicyId.ToLower())
                {
                    return RedirectToAction("Index", "Profile", new { area = "Manage" });
                }
            }
            return View();
        }

        /// <summary>
        ///Grab the short URL and look it up for redirection
        ///see MVC routes for definition
        ///(if we wanted to authenticate the user that is trying to expand a link, here's where we'd do it)
        /// </summary>
        /// <param name="shortUrl"></param>
        /// <returns></returns>
        public ActionResult Redir(string id)
        {
            if (id == null)
                return View("Index");

            var res = ShortUrlUtils.RetrieveUrlFromDatabase(id, Wutcontext);

            if (res == null || (res!=null && res.RealUrl == null)) {
                ViewBag.Error="Sorry, that URL wasn't found. Please check your source and try again.";
                return View("Index");
            }
            if (res.IsProtected)
            {
                //redirect to the authorized entry point
                return Redirect("/a/" + id);
            }

            if (res.IsAzureBlob)
            {
                res = GenToken(res);
            }

            return LogAndRedir(res);
        }

        /// <summary>
        ///Grab the short URL and look it up for redirection
        ///see MVC routes for definition
        ///(if we wanted to authenticate the user that is trying to expand a link, here's where we'd do it)
        /// </summary>
        /// <param name="shortUrl"></param>
        /// <returns></returns>
        public ActionResult RedirAuth(string id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Signin", "Account", new { redir = "/a/" + id });
            }

            if (id == null)
                return View("Index");

            var userId = User.Identity.GetClaim<int>(CustomClaimTypes.UserId);
            var email = User.Identity.GetClaim(ClaimTypes.Email);
            var res = ShortUrlUtils.RetrieveUrlFromDatabase(id, Wutcontext, userId, email);

            if (res == null || (res != null && res.RealUrl == null))
            {
                ViewBag.Error = "Sorry, that URL wasn't found or access is denied. Please check your source and try again.";
                return View("Index");
            }

            //checking to see if this user was granted access to this file
            var isAuth = res.UserEmailColl.Any(u => u == email);
            if (!isAuth)
            {
                Logging.WriteMessageToErrorLog(String.Format("User \"{0}\" attempted unauthorized retrieval of url \"{1}\" resolving to \"{2}\".", email, res.ShortUrl, res.RealUrl), Wutcontext);
                ViewBag.Error = "Sorry, you are not authorized to view this resource. This attempt has been logged. Please check with the issuer.";
                return View("Index");
            }

            if (res.IsAzureBlob)
            {
                res = GenToken(res);
            }

            return LogAndRedir(res, userId);
        }

        private WutLinkPoco GenToken(WutLinkPoco res)
        {
            var containerName = GetContainerFromUri(res.RealUrl);
            var container = WutStorage.GetContainer(containerName);
            res.RealUrl = WutStorage.GetBlobReadTokenUri(container, System.IO.Path.GetFileName(res.RealUrl));
            return res;
        }

        private string GetContainerFromUri(string realUrl)
        {
            var uri = new Uri(realUrl);
            int segment = (realUrl.IndexOf("devstoreaccount")>-1) ? 2 : 1;
            var containerName = uri.Segments[segment];
            containerName = containerName.Substring(0, containerName.Length - 1);
            return containerName;
        }

        private ActionResult LogAndRedir(WutLinkPoco res, int? userId=null)
        {
            var cli = WutQueue.GetQueueClient();
            var queue = WutQueue.GetQueue(cli, Settings.AuditQueueName);
            var msg = new AuditDataModel
            {
                UserId = userId,
                CallDate = DateTime.UtcNow,
                HostIp = Request.UserHostAddress,
                ShortUrl = res.ShortUrl
            };

            var message = JsonConvert.SerializeObject(msg);
            WutQueue.AddMessage(queue, message);

            if (res.UseDelay)
            {
                ViewBag.RealUrl = res.RealUrl;
                return View("redir");
            }
            else
            {
                return Redirect(res.RealUrl);
            }
        }

        public ActionResult Missing()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Mother of All Azure Demos (MOAAD)";

            return View();
        }

        public ActionResult Error(string message)
        {
            if (message == null) message = "N/A";
            ViewBag.ErrorMessage = message.Replace("\r\n","<br>");
            return View();
        }

        public ActionResult Tos()
        {
            return View();
        }

        public ActionResult Privacy()
        {
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "These are not the droids you're looking for.";

            return View();
        }

        public ActionResult IssueInfo()
        {
            return View();
        }

        [HttpPost]
        public ActionResult IssueInfo(string comments)
        {
            if (HttpContext.Session == null) return View();

            var eid = (HttpContext.Session != null && HttpContext.Session["ErrorID"] != null)
                ? HttpContext.Session["ErrorID"].ToString()
                : Request.Form["et"];

            //var emgr = new ErrorMgr(HttpContext.GetOwinContext().Get<WutNuContext>("WutNuContext"), HttpContext);

            var emgr = new ErrorMgr(Wutcontext, HttpContext);
            try
            {
                var eo = emgr.ReadError(eid);
                if (eo != null)
                {
                    eo.UserComment = comments;
                    emgr.SaveError(eo);
                }
                else
                {
                    //Writing to node WEL
                    emgr.WriteToAppLog("Unable to save user comments to. Comment: " + comments, System.Diagnostics.EventLogEntryType.Error);
                }
            }
            catch (Exception ex)
            {
                //Writing to node WEL
                emgr.WriteToAppLog("Unable to save user comments. \r\nError: " + ex.Message + ". \r\nComment: " + comments, System.Diagnostics.EventLogEntryType.Error);
            }

            return View();
        }
    }
}