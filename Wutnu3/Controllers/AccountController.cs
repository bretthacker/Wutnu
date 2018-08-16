using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

// The following using statements were added for this sample.
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using System.Security.Claims;
using Wutnu.Common;

namespace Wutnu.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Signin()
        {
            ViewBag.Redir = Request["redir"];

            return View();
        }
        public ActionResult Signup()
        {
            ViewBag.Redir = Request["redir"];

            return View();
        }
        public ActionResult SignInWorkMulti()
        {
            var redir = Request["redir"];

            if (!Request.IsAuthenticated)
            {
                HttpContext.GetOwinContext().Authentication.Challenge(
                    new AuthenticationProperties
                    {
                        RedirectUri = redir,
                    },
                    WutAuthTypes.B2EMulti);
                return null;
            }

            return Redirect("/");
        }
        public ActionResult SignInWorkGuest()
        {
            var redir = Request["redir"];

            if (!Request.IsAuthenticated)
            {
                HttpContext.GetOwinContext().Authentication.Challenge(
                    new AuthenticationProperties
                    {
                        RedirectUri = redir,
                    },
                    WutAuthTypes.B2B);
                return null;
            }
            return Redirect("/");
        }

        public ActionResult SignInUnified()
        {
            return UnifiedAuth(Startup.UnifiedSusiPolicyId, false);
        }
        public ActionResult ProfileUnified()
        {
            return UnifiedAuth(Startup.UnifiedProfilePolicyId, true);
        }

        public ActionResult SignInB2C()
        {
            return B2CAuth(Startup.SusiPolicyId, false);
        }

        public ActionResult SignUpB2C()
        {
            return B2CAuth(Startup.SusiPolicyId, false);
        }

        public new void Profile()
        {
            B2CAuth(Startup.ProfilePolicyId, true);
        }

        public void PasswordReset()
        {
            B2CAuth(Startup.ResetPolicyId, true);
        }

        private ActionResult B2CAuth(string policyId, bool reqAuth)
        {
            if ((reqAuth && Request.IsAuthenticated) || (!reqAuth && !Request.IsAuthenticated))
            {
                var redir = Request["redir"];

                HttpContext.GetOwinContext().Authentication.Challenge(
                            new AuthenticationProperties() { RedirectUri = redir }, policyId, WutAuthTypes.B2C);
                return null;
            }
            return Redirect("/");
        }
        private ActionResult UnifiedAuth(string policyId, bool reqAuth)
        {
            if ((reqAuth && Request.IsAuthenticated) || (!reqAuth && !Request.IsAuthenticated))
            {
                var redir = Request["redir"];

                HttpContext.GetOwinContext().Authentication.Challenge(
                            new AuthenticationProperties() { RedirectUri = redir }, policyId, WutAuthTypes.Unified);
                return null;
            }
            return Redirect("/");
        }

        public void SignOut()
        {
            Response.Cookies.Clear();
            if (Request.IsAuthenticated)
            {
                var authType = User.Identity.GetClaim(CustomClaimTypes.AuthType);
                if (authType == WutAuthTypes.B2C.ToString())
                {
                    // To sign out the user, you should issue an OpenIDConnect sign out request
                    IEnumerable<AuthenticationDescription> authTypes = HttpContext.GetOwinContext().Authentication.GetAuthenticationTypes().Where(a => !a.AuthenticationType.StartsWith("OIDC"));
                    HttpContext.GetOwinContext().Authentication.SignOut(authTypes.Select(t => t.AuthenticationType).ToArray());
                    Request.GetOwinContext().Authentication.GetAuthenticationTypes();
                }
                else
                {
                    Dictionary<string, string> dict = new Dictionary<string, string>();
                    HttpContext.GetOwinContext().Authentication.SignOut(
                        new AuthenticationProperties(dict),
                        authType,
                        CookieAuthenticationDefaults.AuthenticationType);
                }
            } else
            {
                Response.Redirect("/");
            }
        }
	}
}