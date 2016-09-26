using Autofac;
using Autofac.Integration.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Wutnu.Infrastructure;

namespace Wutnu
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Redir",
                url: "{id}",
                defaults: new { controller = "Home", action = "Redir" } //,
                //constraints: new ShortUrlConstraint(8, "")
            );

            routes.MapRoute(
                name: "Healthcheck",
                url: "healthcheck",
                defaults: new { controller = "Healthcheck", action = "Index" }
                );

            routes.MapRoute(
                name: "RedirAuth",
                url: "a/{id}",
                defaults: new { controller = "Home", action = "RedirAuth" } //,
                //constraints: new ShortUrlConstraint(8, "")
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
