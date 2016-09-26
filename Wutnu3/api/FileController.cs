using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using Wutnu.Common;
using Wutnu.Data;
using Wutnu.Infrastructure;
using Wutnu.Repo;

namespace Wutnu.Web.api
{
    [Authorize]
    public class FileController : BaseApiController
    {
        private readonly string containerName;

        public FileController(WutCache cache, WutNuContext models)
            :base(cache, models)
        {
            containerName = OwnerOID;
        }

        public IEnumerable<IListBlobItem> GetFiles()
        {
            var container = WutStorage.GetContainer(containerName);
            return WutStorage.GetBlobs(container);
        }

        [HttpGet]
        //public string GetUploadSAS(FilePoco poco)
        public HttpResponseMessage GetUploadSAS(string bloburi, string _method, string qqtimestamp)
        {
            var container = WutStorage.GetContainer(containerName);
            var uri = WutStorage.GetBlobWriteTokenUri(container, bloburi);

            //creating implicit response object to force returning a plain string
            var res = new HttpResponseMessage(HttpStatusCode.OK);
            res.Content = new StringContent(uri, System.Text.Encoding.UTF8, "text/plain");
            return res;
        }
        [HttpPost]
        public HttpResponseMessage DeleteBlob(FilePoco file)
        {
            var container = WutStorage.GetContainer(containerName);
            var filePart = file.bloburi.Split(new string[] { containerName }, StringSplitOptions.None);
            if (filePart.Length != 2)
            {
                throw new Exception("File not found or access denied.");
            }

            var deleted = WutStorage.DeleteBlob(container, HttpUtility.UrlDecode(filePart[1]));
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, WutStorage.GetBlobs(container));
            response.Headers.Add("FileDeleted", deleted.ToString());

            return response;
        }
    }
    public class FilePoco
    {
        public string bloburi { get; set; }
        public string _method { get; set; }
        //public string _bloburi { get; set; }
        public string qqtimestamp { get; set; }
    }
}
