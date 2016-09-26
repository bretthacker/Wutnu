using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using Microsoft.Owin;
using Wutnu.Common;

namespace Wutnu.Infrastructure
{
    public static class SiteUtils
    {
        public static void KillSession(HttpContextBase ctx)
        {
            var user = ctx.User as ClaimsPrincipal;
            Debug.Assert(user != null, "user != null");
            var identity = user.Identity as ClaimsIdentity;
            ctx.GetOwinContext().Authentication.User.Claims.ForEach(c =>
            {
                Debug.Assert(identity != null, "identity != null");
                identity.RemoveClaim(c);
            });

            try
            {
                foreach (var cookie in ctx.Request.Cookies.AllKeys.Select(c => new HttpCookie(ctx.Server.UrlEncode(c))
                {
                    Expires = DateTime.Now.AddDays(-1)
                }))
                {
                    ctx.Response.Cookies.Add(cookie);
                }
            }
            catch(Exception ex1)
            {
                Logging.WriteDebugInfoToErrorLog("Cookie reset failed during KillSession", ex1);
                try
                {
                    ctx.Request.Cookies.Clear();
                }
                catch (Exception ex2)
                {
                    Logging.WriteDebugInfoToErrorLog("Cookie clear failed during KillSession", ex2);
                }
            }

            if (ctx.Session!=null) ctx.Session.Abandon();
        }
        /// <summary>
        /// returns HTML indicating that the user has not been authorized for the site
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static string GetUnauthInfoHtml(IOwinContext ctx)
        {
            //ctx.Authentication.SignOut();

            const string loginUrl = "/account/login?reset=true";

            var res = new StringBuilder(String.Format("<!DOCTYPE html><html><head><title>{0} - Unauthorized Access</title>", Utils.ApplicationName));
            res.AppendLine("<style type='text/css'>body, div, h2, td {font-family: arial, helvetica;}</style></head>");
            res.AppendLine("<body>");
            res.AppendLine("<h2>Unauthorized Access</h2>");
            res.AppendLine(String.Format("<div>Sorry, you have not been authorized for {0}.<br>Please contact an administrator to request access.</div>", Utils.ApplicationName));
            res.AppendLine("<a href='" + loginUrl + "'>Refresh Credentials</a>");
            res.AppendLine("</body></html>");
            return res.ToString();
        }

        public static IEnumerable<SelectListItem> GetListFromEnum<T>(T defaultSelected) where T : IConvertible
        {
            IList<SelectListItem> res = (from object o in Enum.GetValues(typeof(T))
                                         select new SelectListItem
                                         {
                                             Selected = (o.ToString() == defaultSelected.ToString(CultureInfo.InvariantCulture)),
                                             Text = Utils.SplitCamelCase(o.ToString()),
                                             Value = Enum.GetName(typeof(T), o)
                                         }).ToList();

            return res.ToList();
        }

        public static string GetRedirPath(int httpCode)
        {
            switch (httpCode)
            {
                case 404:
                    return Settings.Redir404;
                case 403:
                    return Settings.Redir403;
                default:
                    return Settings.Redir500;
            }
        }

        public static void AddAjaxError(ref HttpContextBase htx, string message, HttpStatusCode code)
        {
            htx.Response.StatusCode = (int)code;
            htx.Response.StatusDescription = message;
            htx.Response.AppendHeader("AjaxResponseError", message);
        }

        /// <summary>
        /// Given an HttpContext and an httpCode, rewrite the path and transfer the request to the appropriate handler (403, 404, 500 default)
        /// </summary>
        /// <param name="context">the context</param>
        /// <param name="httpCode">the code</param>
        public static void ReturnViaCode(HttpContextBase context, int httpCode)
        {
            var redir = GetRedirPath(httpCode);
            context.Response.StatusCode = httpCode;
            context.RewritePath(redir, false);
            context.Server.TransferRequest(redir);
        }

        public static string GenApiKey()
        {
            return Guid.NewGuid().ToString().Replace("-", "");
        }
    }
}