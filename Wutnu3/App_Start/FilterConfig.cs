using System.Web;
using System.Web.Mvc;
using Wutnu.Common.Exceptions;

namespace Wutnu
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleAndLogErrorAttribute());
        }
    }
}
