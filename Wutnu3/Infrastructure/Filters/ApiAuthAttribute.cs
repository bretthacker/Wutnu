using Autofac.Integration.Mvc;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Mvc;
using Wutnu.Common;
using Wutnu.Data;
using Wutnu.Data.Models;

namespace Wutnu.Infrastructure.Filters
{
    public class ApiAuth: System.Web.Http.AuthorizeAttribute
    {

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            base.HandleUnauthorizedRequest(actionContext);
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            try
            {
                var key = actionContext.Request.Headers.SingleOrDefault(h => h.Key=="apikey").Value.FirstOrDefault();
                
                if (key == null)
                {
                    Unauthorized(actionContext);
                    return;
                }
                var cache = actionContext.Request.GetDependencyScope().GetService(typeof(WutCache)) as WutCache;

                var user = cache.GetUserFromApiKey(key.ToString());
                if (user == null)
                {
                    Unauthorized(actionContext);
                    return;
                }
                AuthAndAddClaims(user, actionContext);

            }
            catch (Exception ex)
            {
                throw new Exception("Error authorizing access", ex);
            }
        }

        private static void Unauthorized(HttpActionContext actionContext)
        {
            actionContext.Response.StatusCode = System.Net.HttpStatusCode.Forbidden;
            actionContext.Response.ReasonPhrase = "ApiKey is invalid.";
        }

        private static bool AuthAndAddClaims(UserPoco user, HttpActionContext context)
        {
            try
            {
                List<Claim> claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Name, user.PrimaryEmail),
                    new Claim(ClaimTypes.Email, user.PrimaryEmail),
                    new Claim(CustomClaimTypes.UserId, user.UserId.ToString()),
                    new Claim(CustomClaimTypes.ObjectIdentifier, user.UserOID),
                };

                // create an identity with the valid claims.
                ClaimsIdentity identity = new ClaimsIdentity(claims, WutAuthTypes.Api);

                // set the context principal.
                context.RequestContext.Principal = new ClaimsPrincipal(new[] { identity });
                return true;

            }
            catch (Exception ex)
            {
                Logging.WriteDebugInfoToErrorLog("Auth Error", ex);
                return false;
            }
        }

    }
}
