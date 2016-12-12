using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Wutnu.Common
{
    public static class Settings
    {
        public static string AppRootPath = HttpContext.Current.Server.MapPath("//");
        public static string StorageConnectionString { get; set; }
        public static string AuditQueueName { get; set; }
        public static string EmailDomain { get; set; }
        public static string DomainName { get; set; }
        public static EnvType Environment { get; set; }
        public static bool LocalReports { get; set; }

        public static SiteMode SiteMode { get; set; }

        public static DateTime StartTimeUtc { get; set; }

        public const string Redir403 = "~/Home/Unauthorized";

        public const string Redir404 = "~/Home/NotFound";

        public const string Redir500 = "~/Home/Error";

        public static void Setup(NameValueCollection settings)
        {
            DomainName = settings["DomainName"];
            StorageConnectionString = settings["StorageConnectionString"];
            Environment = (EnvType)Enum.Parse(typeof(EnvType), settings["Environment"]);
            AuditQueueName = string.Format("auditqueue{0}", Environment).ToLower();
            LocalReports = Convert.ToBoolean(settings["LocalReports"]);
        }
        public static string GetMailTemplate(string templateName)
        {
            var mailPath = Path.Combine(AppRootPath, @"Data\" + templateName);
            return File.ReadAllText(mailPath);
        }
    }
    public enum SiteMode
    {
        Production,
        Maintenance
    }
    public enum EnvType
    {
        Dev,
        Int,
        QA,
        UAT,
        Prod
    }
}
