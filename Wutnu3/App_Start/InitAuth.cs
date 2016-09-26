using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Web;
using Microsoft.Ajax.Utilities;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Wutnu.Data;
using System.Web.Mvc;
using Wutnu.Common;
using Newtonsoft.Json;
using Wutnu.Data.Models;
using Wutnu.Business;
using Wutnu.Infrastructure;

namespace Wutnu.App_Start
{
    public static class StartupAuth
    {
        public static ClaimsIdentity InitAuth(CookieResponseSignInContext ctx)
        {
            var ident = ctx.Identity;
            var hctx =
                (HttpContextWrapper)
                    ctx.Request.Environment.Single(e => e.Key == "System.Web.HttpContextBase").Value;
            var wutContext = DependencyResolver.Current.GetService<WutNuContext>();

            return InitAuth(ident, hctx, wutContext);
        }

        private static ClaimsIdentity InitAuth(ClaimsIdentity ident, HttpContextBase hctx, WutNuContext wutContext)
        {
            try
            {
                var claimEmail = ident.Claims.SingleOrDefault(c => c.Type == ClaimTypes.Email);
                var claimName = ident.Claims.SingleOrDefault(c => c.Type == ClaimTypes.Name);
                var loginString = (claimEmail != null) ? claimEmail.Value : (claimName != null) ? claimName.Value : null;

                //var user = UserBL.GetActiveUserByEmailOrName(efctx, loginString);
                //if (user == null)
                //{
                //    //user not found
                //    ident.AddClaim(new Claim(ClaimTypesLW.LWUnauthorized, "true"));
                //    return ident;
                //}

                var user = GetUser(ident, loginString, wutContext);
                ident = TransformClaims(ident, user);

                return ident;
            }
            catch (Exception ex)
            {
                Debug.Assert(hctx.Session != null, "hctx.Session != null");
                hctx.Session["AuthError"] = "There was an error authenticating. Please contact the system administrator.";
                Logging.WriteDebugInfoToErrorLog("Error during InitAuth.", ex,wutContext, hctx);
                throw;
            }
        }
        private static User GetUser(ClaimsIdentity ident, string loginString, WutNuContext wutContext)
        {
            var oid = ident.Claims.SingleOrDefault(c => c.Type == CustomClaimTypes.ObjectIdentifier).Value;
            var user = wutContext.Users.SingleOrDefault(u => u.UserOID == oid);
            
            if (user == null)
            {
                user = new User
                {
                    UserOID = oid,
                    PrimaryEmail = loginString,
                    ApiKey = SiteUtils.GenApiKey()
                };

                user = wutContext.Users.Add(user);

                //update any existing user assignments with the new userid
                var assignments = wutContext.UserAssignments.Where(u => u.UserEmail == user.PrimaryEmail && u.UserId == null);
                assignments.ForEach(a =>
                {
                    a.UserId = user.UserId;
                });

                wutContext.SaveChanges();
            }
            return user;
        }

        private static ClaimsIdentity TransformClaims(ClaimsIdentity ident, User user)
        {
            ident.AddClaim(new Claim(CustomClaimTypes.UserId, user.UserId.ToString(), ClaimValueTypes.Integer32));
            if (user.ExtClaims!=null && user.ExtClaims.Length > 0)
            {
                ident.AddClaim(new Claim(CustomClaimTypes.ExtClaims, user.ExtClaims, "ExtClaimsModel"));
            }

            var issuer = ident.Claims.First().Issuer;

            if (issuer.IndexOf("login.microsoftonline.com")>-1)
            {
                //B2C
                var name = ident.Claims.Single(c => c.Type == ClaimTypes.GivenName).Value;
                name += " " + ident.Claims.Single(c => c.Type == ClaimTypes.Surname).Value;
                name += " (" + ident.Claims.Single(c => c.Type == CustomClaimTypes.IdentityProvider).Value + ")";

                var email = ident.Claims.Single(c => c.Type == "emails").Value;
                ident.AddClaim(new Claim(CustomClaimTypes.AuthType, WutAuthTypes.B2C));
                ident.AddClaim(new Claim(ClaimTypes.Email, email));

                //nameidentifier already exists
                ident.AddClaim(new Claim(CustomClaimTypes.FullName, name));
                ident.AddClaim(new Claim(ClaimTypes.Name, email));
                //ident.AddClaim(new Claim(ClaimTypes.Name, ident.Claims.Single(c => c.Type==ClaimTypes.Email).Value));
            }
            else
            {
                //B2B
                ident.AddClaim(new Claim(CustomClaimTypes.AuthType, WutAuthTypes.B2B));

                var groups = ident.Claims.Where(c => c.Type == "groups").ToList();

                var myGroups = AADGraph.GroupList.Where(g => groups.Any(gg => gg.Value == g.ObjectId)).ToList();

                foreach(var group in myGroups)
                {
                    ident.AddClaim(new Claim(ClaimTypes.Role, group.DisplayName));
                }

                var fullName = ident.Claims.Single(c => c.Type == "name").Value;
                ident.AddClaim(new Claim(CustomClaimTypes.FullName, fullName));

                //ident.SetClaim(ClaimTypes.Name, user.UserName);
                //ident.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.UserId));
                //ident.AddClaim(new Claim(ClaimTypesLW.InternalOrExternal, user.InternalOrExternal));
            }

            //ident.AddClaim(new Claim(ClaimTypesLW.ResetPasswordOnLogin, user.ResetOnNextLogin.ToString(), typeof(Boolean).ToString()));

            //var enumerable = userRoles as IList<GetLoginUserRole> ?? userRoles.ToList();
            //enumerable.ForEach(r => ident.AddClaim(new Claim(ClaimTypes.Role, r.Name)));

            return ident;
        }
    }
}