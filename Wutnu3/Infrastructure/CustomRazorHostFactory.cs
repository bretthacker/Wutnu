using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc.Razor;
using System.Web.WebPages.Razor;

namespace Wutnu.Web.Infrastructure
{
    /// <summary>
    /// Enables debugging Razor files that have been loaded via a virtual path provider
    /// http://stackoverflow.com/questions/10633329/cannot-debug-embeddedresource-views-loaded-via-custom-virtualpathprovider
    /// </summary>
    public class MyCustomRazorHostFactory : WebRazorHostFactory
    {
        public override WebPageRazorHost CreateHost(string virtualPath, string physicalPath)
        {
            // Implementation stolen from MvcRazorHostFactory :)
            var host = base.CreateHost(virtualPath, physicalPath);

            if (!host.IsSpecialPage)
            {
                return new MyCustomRazorHost(virtualPath, physicalPath);
            }

            return host;
        }
    }

    public class MyCustomRazorHost : MvcWebPageRazorHost
    {
        public MyCustomRazorHost(string virtualPath, string physicalPath)
            : base(virtualPath, physicalPath)
        {
            if (MyMagicHelper.IsEmbeddedFile(virtualPath))
            {
                PhysicalPath = MyMagicHelper.GetPhysicalFilePath(virtualPath);
            }
        }
    }

    public static class MyMagicHelper
    {
        public static bool IsEmbeddedFile(string virtualPath)
        {
            // ... check if the path is an embedded file path
            return (virtualPath.StartsWith("~/Blob/Report/"));
        }

        public static string GetPhysicalFilePath(string virtualPath)
        {
            // ... resolve the virtual file and return the correct physical file path
            var fileName = VirtualPathUtility.GetFileName(virtualPath);
            return HttpContext.Current.Server.MapPath("/reportslocal/") + fileName;
        }
    }
}