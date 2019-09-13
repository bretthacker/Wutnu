// The following using statements were added for this sample
using Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using System.Threading.Tasks;
using Microsoft.Owin.Security.Notifications;
using Microsoft.IdentityModel.Protocols;
using System.Web.Mvc;
using System.Configuration;
using Wutnu.App_Start;
using Wutnu.Common;
using System.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Web;
using Wutnu.Data;
using System.Linq;

namespace Wutnu
{
	public partial class Startup
	{
        private const string discoverySuffix = "/.well-known/openid-configuration";
        public const string AcrClaimType = "http://schemas.microsoft.com/claims/authnclassreference";

        // App config settings
        private static string clientIdB2B = ConfigurationManager.AppSettings["ida:ClientIdB2B"];
        private static string tenantB2B = ConfigurationManager.AppSettings["ida:TenantB2B"];

        private static string clientId = ConfigurationManager.AppSettings["ida:ClientIdB2C"];
        private static string aadInstanceMulti = ConfigurationManager.AppSettings["ida:AadInstanceMulti"];
        private static string aadInstanceB2B = ConfigurationManager.AppSettings["ida:AadInstanceB2B"];
        private static string aadInstanceB2C = ConfigurationManager.AppSettings["ida:AadInstanceB2C"];
        private static string tenant = ConfigurationManager.AppSettings["ida:TenantB2C"];

        public static string RedirectUri { get; set; }

        // B2C policy identifiers
        public static string SusiPolicyId = ConfigurationManager.AppSettings["ida:SUSIPolicyId"];
        public static string ProfilePolicyId = ConfigurationManager.AppSettings["ida:UserProfilePolicyId"];
        public static string ResetPolicyId = ConfigurationManager.AppSettings["ida:ResetPolicyId"];

        // B2C Unified policy identifiers
        public static string UnifiedSusiPolicyId = ConfigurationManager.AppSettings["ida:UnifiedSUSIPolicyId"];
        public static string UnifiedProfilePolicyId = ConfigurationManager.AppSettings["ida:UnifiedUserProfilePolicyId"];

        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            var authProvider = new CookieAuthenticationProvider
            {
                OnResponseSignIn = ctx =>
                {
                    ctx.Identity = StartupAuth.InitAuth(ctx);
                }
            };
            var cookieOptions = new CookieAuthenticationOptions
            {
                Provider = authProvider
            };

            app.UseCookieAuthentication(cookieOptions);

            // Required for AAD B2C
            // Configure OpenID Connect middleware for each policy
            app.UseOpenIdConnectAuthentication(CreateOptionsFromPolicy(ProfilePolicyId));
            app.UseOpenIdConnectAuthentication(CreateOptionsFromPolicy(SusiPolicyId));
            app.UseOpenIdConnectAuthentication(CreateOptionsFromPolicy(ResetPolicyId));

            if (UnifiedSusiPolicyId != null)
            {
                // Required for B2C Unified
                app.UseOpenIdConnectAuthentication(CreateOptionsFromPolicy(UnifiedSusiPolicyId));
                app.UseOpenIdConnectAuthentication(CreateOptionsFromPolicy(UnifiedProfilePolicyId));
            }

            // Required for AAD B2E Multitenant
            OpenIdConnectAuthenticationOptions multiOptions = new OpenIdConnectAuthenticationOptions
            {
                Authority = aadInstanceMulti,
                ClientId = clientIdB2B,
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    AuthenticationFailed = AuthenticationFailed,
                    RedirectToIdentityProvider = (context) =>
                    {
                        string appBaseUrl = context.Request.Scheme + "://" + context.Request.Host + context.Request.PathBase;
                        context.ProtocolMessage.RedirectUri = appBaseUrl + "/";
                        context.ProtocolMessage.PostLogoutRedirectUri = appBaseUrl;
                        return Task.FromResult(0);
                    },
                },
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                },
                AuthenticationType = WutAuthTypes.B2EMulti,
            };
            app.UseOpenIdConnectAuthentication(multiOptions);

            // Required for AAD Host Tenant/B2B
            OpenIdConnectAuthenticationOptions b2bOptions = new OpenIdConnectAuthenticationOptions
            {
                Authority = string.Format(aadInstanceB2B, tenantB2B),
                ClientId = clientIdB2B,
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    AuthenticationFailed = AuthenticationFailed,
                    RedirectToIdentityProvider = (context) =>
                    {
                        string appBaseUrl = context.Request.Scheme + "://" + context.Request.Host + context.Request.PathBase;
                        context.ProtocolMessage.RedirectUri = appBaseUrl + "/";
                        context.ProtocolMessage.PostLogoutRedirectUri = appBaseUrl;
                        return Task.FromResult(0);
                    },
                },
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                },
                AuthenticationType = WutAuthTypes.B2B,
            };
            app.UseOpenIdConnectAuthentication(b2bOptions);
        }

        private async Task AuthenticationFailed(AuthenticationFailedNotification<Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification)
        {
            notification.HandleResponse();
            string message = notification.ProtocolMessage.ErrorDescription ?? notification.Exception.Message;

            var form = await notification.Request.ReadFormAsync();
            var desc = form.GetValues("error_description")[0].ToString();
            message = string.Format("{0} \r\n ({1})", message, desc);
            var hctx =
                (HttpContextWrapper)
                    notification.Request.Environment.Single(e => e.Key == "System.Web.HttpContextBase").Value;
            var wutContext = DependencyResolver.Current.GetService<WutNuContext>();

            Logging.WriteDebugInfoToErrorLog(message, notification.Exception, wutContext, hctx);
            notification.Response.Redirect("/Home/Error?message=" + message);
        }

        private OpenIdConnectAuthenticationOptions CreateOptionsFromPolicy(string policy)
        {
            return new OpenIdConnectAuthenticationOptions
            {
                // For each policy, give OWIN the policy-specific metadata address, and
                // set the authentication type to the id of the policy
                MetadataAddress = string.Format(aadInstanceB2C, tenant, policy),
                AuthenticationType = policy,

                // These are standard OpenID Connect parameters, with values pulled from web.config
                ClientId = clientId,
                RedirectUri = RedirectUri,
                PostLogoutRedirectUri = RedirectUri,
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    AuthenticationFailed = AuthenticationFailed,
                    RedirectToIdentityProvider = (context) =>
                    {
                        string appBaseUrl = context.Request.Scheme + "://" + context.Request.Host + context.Request.PathBase;
                        context.ProtocolMessage.RedirectUri = appBaseUrl + "/";
                        context.ProtocolMessage.PostLogoutRedirectUri = appBaseUrl;
                        return Task.FromResult(0);
                    }
                },
                Scope = "openid",
                ResponseType = "id_token"
            };
        }
    }
}
 