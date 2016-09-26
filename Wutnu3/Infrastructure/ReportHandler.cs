using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using Wutnu.Common;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure;
using Wutnu.Repo;

namespace Wutnu.InfraStructure
{
    public class ReportHandler : IHttpHandler, IRequiresSessionState
    {
        /// <summary>
        /// You will need to configure this handler in the Web.config file of your 
        /// web and register it with IIS before being able to use it. For more information
        /// see the following link: http://go.microsoft.com/?linkid=8101007
        /// </summary>
        #region IHttpHandler Members

        public bool IsReusable
        {
            // Return false in case your Managed Handler cannot be reused for another request.
            // Usually this would be false in case you have some state information preserved per request.
            get { return true; }
        }

        /// <summary>
        /// Processes the request.
        /// </summary>
        /// <param name="context">The context.</param>
        public void ProcessRequest(HttpContext context)
        {
            string contentType;
            string name = context.Request.Url.AbsolutePath.Replace("/reportengine/", "");
            Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(Settings.StorageConnectionString);
            byte[] data;
            if (!Settings.LocalReports)
            {
                //BlobBusinessLayer blobBusinessLayer = new BlobBusinessLayer(new HttpContextWrapper(context));
                var container = WutStorage.GetContainer("reports");
                var blobFile = WutStorage.GetBlob(container, name);
                data = new byte[blobFile.Properties.Length];
                contentType = blobFile.Properties.ContentType;
                blobFile.DownloadToByteArray(data, 0);
            }
            else
            {
                var filepath = context.Server.MapPath("/reportslocal/") + name;
                var file = File.Open(filepath,FileMode.Open);
                data=new byte[file.Length];
                file.Read(data, 0, (int)file.Length);
                contentType = (Path.GetExtension(filepath) == ".cshtml") ? "text/html" : "application/octet-stream";
            }
            context.Response.ContentType = contentType;
            context.Response.BinaryWrite(data);
            context.Response.Flush();
        }

        #endregion
    }
}