using Microsoft.Owin;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wutnu.App_Start
{
    public class B2COpenIdConnectAuthenticationMiddleware : OpenIdConnectAuthenticationMiddleware
    {
        private readonly ILogger _logger;

        public B2COpenIdConnectAuthenticationMiddleware(OwinMiddleware next, IAppBuilder app, OpenIdConnectAuthenticationOptions options)
            : base(next, app, options)
        {
            _logger = app.CreateLogger<B2COpenIdConnectAuthenticationMiddleware>();
        }

        protected override AuthenticationHandler<OpenIdConnectAuthenticationOptions> CreateHandler()
        {
            return new B2COpenIdConnectAuthenticationHandler(_logger);
        }
    }
}
