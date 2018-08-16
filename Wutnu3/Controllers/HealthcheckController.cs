using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Wutnu.Data;
using Wutnu.Repo;
using Wutnu.Common;
using System.Data.SqlClient;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Wutnu.Web.Controllers
{
    public class HealthcheckController : Controller
    {
        private bool _bOk = true;
        private StringBuilder _outputMessage = new StringBuilder();
        WutNuContext _context;
        WutCache _cache;

        public HealthcheckController(WutCache cache, WutNuContext models)
        {
            _context = models;
            _cache = cache;
        }
        // GET: Healthcheck
        public void Index()
        {
            ProcessRequest();
        }

        public void ProcessRequest()
        {
            _outputMessage.AppendFormat("<html><head><title>HEALTH CHECK: {0}</title></head><body><pre>",
            HttpContext.Request.Url.GetLeftPart(UriPartial.Authority)).AppendLine();

            _outputMessage.AppendFormat("HEALTH CHECK: {0}", HttpContext.Request.Url.GetLeftPart(UriPartial.Authority)).AppendLine();
            _outputMessage.AppendLine();

            _outputMessage.AppendFormat("Date: {0}, {1} UTC", DateTime.UtcNow.ToShortDateString(), DateTime.UtcNow.ToLongTimeString()).AppendLine();
            _outputMessage.AppendLine();

            _outputMessage.AppendFormat("App Node: {0}, {1}", HttpContext.Server.MachineName, HttpContext.Request.ServerVariables["LOCAL_ADDR"]);
            _outputMessage.AppendLine().AppendLine();

            //DB test
            DbTest();

            //STORAGE TEST
            StorageTest();

            //CACHE TEST
            CacheTest();

            if (!_bOk)
            {
                HttpContext.Response.StatusCode = 500;
                HttpContext.Response.TrySkipIisCustomErrors = true;
                HttpContext.Response.StatusDescription = "Errors were encountered";

                _outputMessage.AppendLine("");
                _outputMessage.AppendLine("One or more errors occured, please check the notes above. (A 500 error was returned if this was a load balancer health check.)");
            }

            _outputMessage.AppendLine("</pre></body></html>");
            _outputMessage.Append((_outputMessage.Length < 513) ? new string(' ', Convert.ToInt32(513 - _outputMessage.Length)) : "");

            HttpContext.Response.Write(_outputMessage.ToString());
        }

        //Test Functions
        private void DbTest()
        {
            SqlConnection conn = null;
            try
            {
                _outputMessage.Append("Testing DB ");
                conn = new SqlConnection();
                conn.ConnectionString = _context.Database.Connection.ConnectionString;
                conn.Open();

                _outputMessage.AppendFormat("\"{0}\"", conn.Database).AppendLine();
                _outputMessage.AppendFormat("     Host: {0}", conn.DataSource).AppendLine();
                _outputMessage.AppendFormat("     Version: {0}", conn.ServerVersion).AppendLine();
            }
            catch (Exception ex)
            {
                _bOk = false;
                AppendError("     DB: error - ", _outputMessage, ex);
            }
            finally
            {
                try
                {
                    conn.Close();
                    conn.Dispose();
                }
                catch { }
            }
            _outputMessage.AppendLine();
        }

        private void StorageTest()
        {
            try
            {
                _outputMessage.Append("Testing Storage Account ");
                var acct = WutStorage.GetStorageAccount();
                _outputMessage.AppendFormat("\"{0}\"", acct.Credentials.AccountName).AppendLine();

                var blobClient = acct.CreateCloudBlobClient();

                var qClient = WutQueue.GetQueueClient();
                var q = WutQueue.GetQueue(qClient, Settings.AuditQueueName);
                var props = blobClient.GetServiceProperties(); 

                _outputMessage.AppendFormat("     Current Queue Count: {0}", q.ApproximateMessageCount).AppendLine();
                _outputMessage.AppendFormat("<blockquote>{0}</blockquote>", WutStorage.GetFormattedServiceProperties(props)).AppendLine();
            }
            catch (Exception ex)
            {
                _bOk = false;
                AppendError("     Azure Storage: unknown error - ", _outputMessage, ex);
            }

            _outputMessage.AppendLine("");
        }

        private void CacheTest()
        {
            try
            {
                _outputMessage.Append("Testing Redis Cache ");
                var host = Cache.Connection.Configuration.Split(',')[0];
                _outputMessage.AppendFormat("\"{0}\"", host).AppendLine();
                _outputMessage.AppendFormat("     Redis connected: {0}", Cache.Connection.IsConnected).AppendLine();
                _outputMessage.AppendFormat("     Current Redis status: <blockquote>{0}</blockquote>", Cache.Connection.GetStatus());
            }
            catch (Exception ex)
            {
                _bOk = false;
                AppendError("     Redis error - ", _outputMessage, ex);
            }
        }

        private static void AppendError(string label, StringBuilder outputMessage, Exception ex)
        {
            outputMessage.AppendLine(label + ex.Message);
            if (ex.InnerException != null)
            {
                outputMessage.AppendLine("(" + ex.GetBaseException() + ")");
            }
        }
    }
}