using System.Web;
using System.Web.Http;
using Wutnu.Data;
using Wutnu.Common;
using System.IdentityModel.Claims;

namespace Wutnu.Infrastructure
{
    public abstract class BaseApiController : ApiController
    {
        protected HttpContextBase WebContextBase;
        protected WutNuContext Wutcontext;
        protected WutCache Wutcache;
        protected string OwnerOID;
        protected int UserId;
        protected string UserEmail;

        protected BaseApiController(WutCache cache, WutNuContext models)
        {
            WebContextBase = new HttpContextWrapper(HttpContext.Current);
            Wutcontext = models;
            Wutcache = cache;
            var uid = WebContextBase.User.Identity;

            if (uid.IsAuthenticated)
            {
                UserId = uid.GetClaim<int>(CustomClaimTypes.UserId);
                OwnerOID = uid.GetClaim(CustomClaimTypes.ObjectIdentifier);
                UserEmail = uid.GetClaim(ClaimTypes.Email);
            }
        }
    }
}