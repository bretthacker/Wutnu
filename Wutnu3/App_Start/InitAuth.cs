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
                string claimEmail = null;
                string claimName = null;
                string loginString = null;

                loginString = ident.GetClaim(ClaimTypes.Upn);
                if (loginString == null)
                {
                    claimEmail = ident.GetClaim("emails");
                    if (claimEmail == null)
                        claimEmail = ident.GetClaim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");
                    if (claimEmail == null)
                        claimEmail = ident.GetClaim(ClaimTypes.Email);
                    if (claimEmail == null)
                        claimName = ident.GetClaim(ClaimTypes.Name);

                    loginString = claimEmail ?? (claimName ?? null);
                }

                if (loginString == null)
                {
                    Logging.WriteDebugInfoToErrorLog("Error during InitAuth.", new Exception("Unable to determine email claim from account."), wutContext, hctx);
                    return null;
                }

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
            //have to check source to get an object ID: AAD is using OID, B2C is using nameidentifier
            string oid = null;

            var issuer = ident.Claims.First().Issuer;
            if (issuer.IndexOf("b2clogin.com") > -1)
            {
                oid = ident.GetClaim(ClaimTypes.NameIdentifier);
                if (!ident.HasClaim(CustomClaimTypes.ObjectIdentifier))
                {
                    ident.AddClaim(new Claim(CustomClaimTypes.ObjectIdentifier, oid));
                }

            }
            else
            {
                oid = ident.GetClaim(CustomClaimTypes.ObjectIdentifier);
            }

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
            }

            user.iss = ident.GetClaim("iss");
            user.idp = ident.GetClaim(CustomClaimTypes.IdentityProvider);

            wutContext.SaveChanges();

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

            if (issuer.IndexOf("b2clogin.com") >-1)
            {
                //B2C
                //todo: is unified?
                ident.AddClaim(new Claim(CustomClaimTypes.AuthType, WutAuthTypes.B2C));


                var name = ident.GetClaim(ClaimTypes.GivenName);
                name += " " + ident.GetClaim(ClaimTypes.Surname);
                name += " (" + user.idp + ")";

                var email = ident.GetClaim("emails") ?? ident.GetClaim(ClaimTypes.Email);
                if (ident.HasClaim(ClaimTypes.Email))
                {
                    ident.SetClaim(ClaimTypes.Email, email);
                }
                else
                {
                    ident.AddClaim(new Claim(ClaimTypes.Email, email));
                }

                ident.AddClaim(new Claim(CustomClaimTypes.FullName, name));
                ident.AddClaim(new Claim(ClaimTypes.Name, email));
            }
            else
            {
                //WORK OR SCHOOL ACCOUNTS
                var userTenant = AADGraph.GetUserTenantId(ident);

                //GetHomeTenantId is null if authenticating in home tenant
                if (ident.GetIdpTenantId() == null)
                {
                    if (ident.GetClaim(CustomClaimTypes.TenantId) != AADGraph.TenantId)
                    {
                        //not in host tenant and auth'd to home tenant
                        ident.AddClaim(new Claim(CustomClaimTypes.AuthType, WutAuthTypes.B2EMulti));
                    }
                    else if (userTenant == AADGraph.TenantId)
                    {
                        //in host tenant, which matches home tenant
                        ident.AddClaim(new Claim(CustomClaimTypes.AuthType, WutAuthTypes.LocalAAD));
                    }
                }
                else
                {
                    //B2B guest
                    ident.AddClaim(new Claim(CustomClaimTypes.AuthType, WutAuthTypes.B2B));
                }

                if (ident.GetClaim(CustomClaimTypes.TenantId) == AADGraph.TenantId)
                {
                    if (AADGraph.AdminUsers == null)
                    {
                        //Admin group was left null - all local tenant users (native only, not B2B) are set as admins
                        if (userTenant == AADGraph.TenantId)
                        {
                            ident.AddClaim(new Claim(ClaimTypes.Role, WutRoles.WutNuAdmins));
                        }
                    } else
                    {
                        //Admin group was configured - all members of the group, native or B2B guests, are set as admins
                        if (AADGraph.AdminUsers.Any(u => u.ObjectId == ident.GetClaim(CustomClaimTypes.ObjectIdentifier)))
                        {
                            ident.AddClaim(new Claim(ClaimTypes.Role, WutRoles.WutNuAdmins));
                        }
                    }
                }

                var fullName = ident.Claims.FirstOrDefault(c => c.Type == "name").Value;
                ident.AddClaim(new Claim(CustomClaimTypes.FullName, fullName));

                //ident.SetClaim(ClaimTypes.Name, user.UserName);
                //ident.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.UserId));
                //ident.AddClaim(new Claim(ClaimTypesLW.InternalOrExternal, user.InternalOrExternal));
            }

            if (!ident.Claims.Any(i => i.Type == ClaimTypes.Email))
            {
                ident.AddClaim(new Claim(ClaimTypes.Email, user.PrimaryEmail));
            }

            //ident.AddClaim(new Claim(ClaimTypesLW.ResetPasswordOnLogin, user.ResetOnNextLogin.ToString(), typeof(Boolean).ToString()));

            //var enumerable = userRoles as IList<GetLoginUserRole> ?? userRoles.ToList();
            //enumerable.ForEach(r => ident.AddClaim(new Claim(ClaimTypes.Role, r.Name)));

            return ident;
        }
    }
}