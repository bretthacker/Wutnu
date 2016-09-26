using System;
using System.Configuration;
using System.Web.Http;
using Wutnu.Common.Exceptions;
using Wutnu.Data;

namespace Wutnu
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            config.Filters.Add(new HandleWebApiException());

            //config.MapHttpAttributeRoutes();

            if (Convert.ToBoolean(ConfigurationManager.AppSettings["ActivateWebApiTracing"]))
                config.EnableSystemDiagnosticsTracing();

            config.Routes.MapHttpRoute(
               name: "ActionAndIdApi",
               routeTemplate: "api/{controller}/{action}/{id}",
               defaults: new { id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
               name: "ActionOnlyApi",
               routeTemplate: "api/{controller}/{action}",
               defaults: new { id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
