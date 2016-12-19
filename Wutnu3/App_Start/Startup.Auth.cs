using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
using System.IdentityModel.Tokens;
using Wutnu.App_Start;
using Wutnu.Common;
using Microsoft.IdentityModel.Tokens;

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
        private static string aadInstance = ConfigurationManager.AppSettings["ida:AadInstance"];
        private static string aadInstanceB2C = ConfigurationManager.AppSettings["ida:AadInstanceB2C"];
        private static string tenant = ConfigurationManager.AppSettings["ida:TenantB2C"];
        //private static string redirectUri = ConfigurationManager.AppSettings["ida:RedirectUri"] + "/";
        public static string RedirectUri { get; set; }

        // B2C policy identifiers
        public static string SignUpPolicyId = ConfigurationManager.AppSettings["ida:SignUpPolicyId"];
        public static string SignInPolicyId = ConfigurationManager.AppSettings["ida:SignInPolicyId"];
        //public static string ResetPolicyId = ConfigurationManager.AppSettings["ida:PasswordResetPolicyId"];
        public static string ProfilePolicyId = ConfigurationManager.AppSettings["ida:UserProfilePolicyId"];
        public static string ResetPolicyId = ConfigurationManager.AppSettings["ida:ResetPolicyId"];

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
            app.UseOpenIdConnectAuthentication(CreateOptionsFromPolicy(SignUpPolicyId));
            app.UseOpenIdConnectAuthentication(CreateOptionsFromPolicy(ProfilePolicyId));
            app.UseOpenIdConnectAuthentication(CreateOptionsFromPolicy(SignInPolicyId));

            //OpenIdConnectAuthenticationOptions b2cOptions = new OpenIdConnectAuthenticationOptions
            //{
            //    // Standard OWIN OIDC parameters
            //    Authority = string.Format(aadInstance, tenant),
            //    ClientId = clientId,
            //    RedirectUri = RedirectUri,
            //    PostLogoutRedirectUri = RedirectUri,
            //    ProtocolValidator = new OpenIdConnectProtocolValidator { RequireNonce=false },
            //    Notifications = new OpenIdConnectAuthenticationNotifications
            //    { 
            //        AuthenticationFailed = AuthenticationFailed,
            //        RedirectToIdentityProvider = (context) =>
            //        {
            //            string appBaseUrl = context.Request.Scheme + "://" + context.Request.Host + context.Request.PathBase;
            //            context.ProtocolMessage.RedirectUri = appBaseUrl + "/";
            //            context.ProtocolMessage.PostLogoutRedirectUri = appBaseUrl;

            //            return Task.FromResult(0);
            //        },
            //    },

            //    Scope = "openid",
            //    ConfigurationManager = new B2CConfigurationManager(string.Format(aadInstance + "{1}", tenant, discoverySuffix)),

            //    // Optional - used for displaying the user's name in the navigation bar when signed in.
            //    TokenValidationParameters = new TokenValidationParameters
            //    {  
            //        NameClaimType = "name",
            //    },

            //    AuthenticationType = WutAuthTypes.B2C,
            //};
            
            //app.Use(typeof(B2COpenIdConnectAuthenticationMiddleware), app, b2cOptions);

            // Required for AAD B2B
            OpenIdConnectAuthenticationOptions b2bOptions = new OpenIdConnectAuthenticationOptions
            {
                Authority = string.Format(aadInstance, tenantB2B),
                ClientId = clientIdB2B,
                //RedirectUri = RedirectUri,
                //PostLogoutRedirectUri = RedirectUri,
                //ProtocolValidator = new OpenIdConnectProtocolValidator { RequireNonce = false },
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
                
                //TokenValidationParameters = new TokenValidationParameters
                //{
                //    ValidateIssuer = false,
                //},

                AuthenticationType = WutAuthTypes.B2B,
            };
            
            app.UseOpenIdConnectAuthentication(b2bOptions);
        }

        // Used for avoiding yellow-screen-of-death
        private Task AuthenticationFailed(AuthenticationFailedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification)
        {
            notification.HandleResponse();
            if (notification.Exception.Message == "access_denied")
            {
                notification.Response.Redirect("/");
            }
            else
            {
                notification.Response.Redirect("/Home/Error?message=" + notification.Exception.Message);
            }

            return Task.FromResult(0);
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
                },
                Scope = "openid",
                ResponseType = "id_token",

                //// This piece is optional - it is used for displaying the user's name in the navigation bar.
                //TokenValidationParameters = new TokenValidationParameters
                //{
                //    NameClaimType = "name",
                //},
            };
        }
    }
}