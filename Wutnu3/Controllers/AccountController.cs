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

        public void SignInB2C()
        {
            if (!Request.IsAuthenticated)
            {
                HttpContext.GetOwinContext().Authentication.Challenge(
                    new AuthenticationProperties(
                        new Dictionary<string, string> 
                        { 
                            {B2COpenIdConnectAuthenticationHandler.PolicyParameter, Startup.SignInPolicyId} 
                        })
                    {
                        RedirectUri = (Request["redir"] == null) ? "/" : Request["redir"],
                    },
                    WutAuthTypes.B2C);
            }
        }

        public void SignInWork()
        {
            if (!Request.IsAuthenticated)
            {
                HttpContext.GetOwinContext().Authentication.Challenge(
                    new AuthenticationProperties
                    {
                        RedirectUri = (Request["redir"] == "") ? "/" : Request["redir"],
                    },
                    WutAuthTypes.B2B);
            }
        }

        public void SignUpB2C()
        {
            if (!Request.IsAuthenticated)
            {
                HttpContext.GetOwinContext().Authentication.Challenge(
                    new AuthenticationProperties(
                        new Dictionary<string, string> 
                        { 
                            {B2COpenIdConnectAuthenticationHandler.PolicyParameter, Startup.SignUpPolicyId} 
                        })
                    {
                        RedirectUri = (Request["redir"] == null) ? "/" : Request["redir"],
                    },
                    WutAuthTypes.B2C);
            }
        }


        public new void Profile()
        {
            if (Request.IsAuthenticated)
            {
                HttpContext.GetOwinContext().Authentication.Challenge(
                    new AuthenticationProperties(
                        new Dictionary<string, string>
                        {
                            {B2COpenIdConnectAuthenticationHandler.PolicyParameter, Startup.ProfilePolicyId}
                        })
                    {
                        RedirectUri = (Request["redir"] == null) ? "/" : Request["redir"],
                    },
                    WutAuthTypes.B2C);
            }
        }

        // Password Reset in Progress
        //public void PasswordReset()
        //{
        //    if (Request.IsAuthenticated)
        //    {
        //        HttpContext.GetOwinContext().Authentication.Challenge(
        //            new AuthenticationProperties(
        //                new Dictionary<string, string> 
        //                { 
        //                    {B2COpenIdConnectAuthenticationHandler.PolicyParameter, Startup.ResetPolicyId} 
        //                })
        //            {
        //                RedirectUri = "/",
        //            },
        //            OpenIdConnectAuthenticationDefaults.AuthenticationType);
        //    }
        //}

        public void SignOut()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            if (ClaimsPrincipal.Current.FindFirst(Startup.AcrClaimType) != null)
            {
                dict[B2COpenIdConnectAuthenticationHandler.PolicyParameter] = ClaimsPrincipal.Current.FindFirst(Startup.AcrClaimType).Value;
                HttpContext.GetOwinContext().Authentication.SignOut(
                    new AuthenticationProperties(dict),
                    WutAuthTypes.B2C, CookieAuthenticationDefaults.AuthenticationType);
            }
            else
            {
                HttpContext.GetOwinContext().Authentication.SignOut(
                    new AuthenticationProperties(dict),
                    WutAuthTypes.B2B, CookieAuthenticationDefaults.AuthenticationType);
            }
            
            
        }
	}
}