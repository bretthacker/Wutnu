using System;
using System.Web.Http;
using Infrastructure;
using System.Linq;
using Wutnu.Data;
using System.Collections;
using System.IdentityModel.Claims;
using System.Collections.Generic;
using Wutnu.Common;
using Wutnu.Infrastructure;

namespace Wutnu.api
{
    /// <summary>
    /// if you decide to do it ajax-y
    /// </summary>
    public class UrlController : BaseApiController
    {

        public UrlController(WutCache cache, WutNuContext models)
            :base(cache, models)
        {
        }

        [Authorize]
        [HttpPost]
        public IEnumerable<WutLinkPoco> SaveUrl(WutLinkPoco oUrl)
        {
            oUrl = ShortUrlUtils.UpdateUrl(oUrl, Wutcontext, UserId);

            return GetUrls();
        }

        [Authorize]
        [HttpPost]
        public IEnumerable<WutLinkPoco> DeleteUrl(WutLinkPoco oUrl)
        {
            ShortUrlUtils.DeleteUrl(oUrl, Wutcontext);
            return GetUrls();
        }

        [HttpPost]
        public WutLinkPoco CreateUrl(dynamic data)
        {
            var oUrl = new WutLink
            {
                RealUrl = data.realUrl,
                IsAzureBlob = data.isBlob,
                CreateDate = DateTime.UtcNow,
                CreatedByIp = ((System.Web.HttpContextWrapper)Request.Properties["MS_HttpContext"]).Request.Se‌​rverVariables["HTTP_HOST"]
            };

            if (User.Identity.IsAuthenticated)
            {
                oUrl.UserId = UserId;
            }

            oUrl = ShortUrlUtils.AddUrlToDatabase(oUrl, Wutcontext);

            return WutLinkPoco.GetPocoFromObject(oUrl);
        }

        public WutLinkPoco GetUrl(string short_url)
        {
            short_url = ShortUrlUtils.InternalShortUrl(short_url);
            return ShortUrlUtils.RetrieveUrlFromDatabase(short_url, Wutcontext);
        }

        [Authorize]
        public IEnumerable<WutLinkPoco> GetUrls()
        {
            try
            {
                var res = Wutcontext.WutLinks.Include("UserAssignments")
                    .Where(s => s.UserId == UserId)
                    .OrderBy(s => s.CreateDate)
                    .Select(WutLinkPoco.GetPocoFromObject)
                    .ToList();

                return res;
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting URLs from API", ex);
            }
        }

        [Authorize]
        [HttpGet]
        public bool IsUniqueShortLink(string shortLinkCandidate)
        {
            return !(Wutcontext.WutLinks.Any(s => s.ShortUrl == shortLinkCandidate));
        }
    }
}
