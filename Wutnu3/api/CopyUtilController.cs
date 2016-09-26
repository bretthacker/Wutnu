using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Wutnu.Common;
using Wutnu.Data;
using Wutnu.Infrastructure;
using Wutnu.Infrastructure.Filters;
using Wutnu.Repo;

namespace Wutnu.Web.api
{
    [ApiAuth]
    public class CopyUtilController : ApiController
    {
        //public CopyUtilController(WutCache cache, WutNuContext models)
        //{
        //}

        private CloudBlobContainer GetContainer()
        {
            //I don't like this but I couldn't assign in ctor because the filter is authenticating me
            var sContainer = User.Identity.GetClaim(CustomClaimTypes.ObjectIdentifier);
            return WutStorage.GetContainer(sContainer);
        }

        public HttpResponseMessage GetUploadToken(string path)
        {
            var uri = WutStorage.GetBlobWriteTokenUri(GetContainer(), path);

            //creating implicit response object to force returning a plain string
            var res = new HttpResponseMessage(HttpStatusCode.OK);
            res.Content = new StringContent(uri, System.Text.Encoding.UTF8, "text/plain");
            return res;
        }

        public HttpResponseMessage GetDownloadToken(string path)
        {
            var uri = WutStorage.GetBlobReadTokenUri(GetContainer(), path);

            //creating implicit response object to force returning a plain string
            var res = new HttpResponseMessage(HttpStatusCode.OK);
            res.Content = new StringContent(uri, System.Text.Encoding.UTF8, "text/plain");
            return res;

        }

        public IEnumerable<IListBlobItem> GetList()
        {
            return WutStorage.GetBlobs(GetContainer());
        }

        [HttpPost]
        public HttpResponseMessage DeleteBlob(string path)
        {
            var deleted = WutStorage.DeleteBlob(GetContainer(), path);

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, GetList());
            response.Headers.Add("FileDeleted", deleted.ToString());

            return response;
        }
    }
}
