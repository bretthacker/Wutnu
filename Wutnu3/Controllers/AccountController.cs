using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

// The following using statements were added for this sample.
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.Owin.Security.Cookies;
using Wutnu.App_Start;
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
            B2CAuth(Startup.SignInPolicyId, false);
       }

        public void SignUpB2C()
        {
            B2CAuth(Startup.SignUpPolicyId, false);
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

                //HttpContext.GetOwinContext().Authentication.Challenge(
                //new AuthenticationProperties(
                //    new Dictionary<string, string>
                //    {
                //        {B2COpenIdConnectAuthenticationHandler.PolicyParameter, policyId}
                //    })
                //{
                //    RedirectUri = "/",
                //},
                //WutAuthTypes.B2C);
            }
        }

        public void SignOut()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            if (ClaimsPrincipal.Current.FindFirst(Startup.AcrClaimType) != null)
            {
                dict[B2COpenIdConnectAuthenticationHandler.PolicyParameter] = ClaimsPrincipal.Current.FindFirst(Startup.AcrClaimType).Value;
                HttpContext.GetOwinContext().Authentication.SignOut(
                    new AuthenticationProperties(dict),
                    WutAuthTypes.B2C, 
                    CookieAuthenticationDefaults.AuthenticationType);
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