using System.Web;
using System.Web.Mvc;
using Wutnu.Data;
using Wutnu.Common;
using System.IdentityModel.Claims;

namespace Wutnu.Infrastructure
{
    public abstract class BaseController : Controller
    {
        protected WutNuContext Wutcontext;
        protected WutCache Wutcache;
        protected string OwnerOID;
        protected string UserEmail;
        protected int UserId;

        protected BaseController(WutCache cache, WutNuContext models)
        {
            try
            {
                Wutcontext = models;
                Wutcache = cache;
                var uid = System.Web.HttpContext.Current.User.Identity;

                if (uid.IsAuthenticated)
                {
                    UserId = uid.GetClaim<int>(CustomClaimTypes.UserId);
                    OwnerOID = uid.GetClaim(CustomClaimTypes.ObjectIdentifier);
                    UserEmail = uid.GetClaim(ClaimTypes.Email);
                }
            }
            catch (System.Exception ex)
            {
                Logging.WriteDebugInfoToErrorLog(ex.Message, ex);
                throw;
            }
        }
    }
}