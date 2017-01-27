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

        public void SignInWork()
        {
            if (!Request.IsAuthenticated)
            {
                HttpContext.GetOwinContext().Authentication.Challenge(
                    new AuthenticationProperties
                    {
                        RedirectUri = "/",
                    },
                    WutAuthTypes.B2B);
            }
        }

        public void SignInB2C()
        {
            B2CAuth(Startup.SusiPolicyId, false);
       }

        public void SignUpB2C()
        {
            B2CAuth(Startup.SusiPolicyId, false);
        }

        public new void Profile()
        {
            B2CAuth(Startup.ProfilePolicyId, true);
        }

        public void PasswordReset()
        {
            B2CAuth(Startup.ResetPolicyId, true);
        }

        private void B2CAuth(string policyId, bool reqAuth)
        {
            if ((reqAuth && Request.IsAuthenticated) || (!reqAuth && !Request.IsAuthenticated))
            {
                HttpContext.GetOwinContext().Authentication.Challenge(
                            new AuthenticationProperties() { RedirectUri = "/" }, policyId, WutAuthTypes.B2C);
            }
        }

        public void SignOut()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            if (ClaimsPrincipal.Current.FindFirst(Startup.AcrClaimType) != null)
            {
                // To sign out the user, you should issue an OpenIDConnect sign out request
                if (Request.IsAuthenticated)
                {
                    IEnumerable<AuthenticationDescription> authTypes = HttpContext.GetOwinContext().Authentication.GetAuthenticationTypes();
                    HttpContext.GetOwinContext().Authentication.SignOut(authTypes.Select(t => t.AuthenticationType).ToArray());
                    Request.GetOwinContext().Authentication.GetAuthenticationTypes();
                }
            }
            else
            {
                HttpContext.GetOwinContext().Authentication.SignOut(
                    new AuthenticationProperties(dict),
                    WutAuthTypes.B2B, 
                    CookieAuthenticationDefaults.AuthenticationType);
            }
        }
	}
}