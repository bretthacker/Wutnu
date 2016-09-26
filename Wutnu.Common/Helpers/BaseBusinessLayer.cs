using System;
using System.Web;
using System.Security.Claims;
using Wutnu.Data;

namespace Wutnu.Common.Helpers
{
    /// <summary>
    /// The base business layer.
    /// </summary>
    public class BaseBusinessLayer: IDisposable
    {
        public WutNuContext io;
        public HttpContextBase htxBase;
        public string UserIO;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseBusinessLayer"/> class.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        public BaseBusinessLayer(HttpContextBase httpContext, WutNuContext context)
        {
            io = context;
            htxBase = httpContext;
            //io.Configuration.ProxyCreationEnabled = false;
            //io.Configuration.AutoDetectChangesEnabled = false;
            io.Configuration.LazyLoadingEnabled = true;

            //Wutcache = httpContext.GetOwinContext().Get<WutCache>("WutCache");
            var ident = httpContext.User.Identity;
            if (ident.IsAuthenticated)
            {
                UserIO = ident.GetClaim(CustomClaimTypes.ObjectIdentifier);
            }
        }

        #region IDisposable Members
        public void Dispose()
        {
        }
        #endregion
    }
}
