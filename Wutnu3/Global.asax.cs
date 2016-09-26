using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Claims;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Wutnu.Business;
using Wutnu.Data;
using Wutnu.Infrastructure;
using Wutnu.Common.ErrorMgr;
using Wutnu.Common;
using Wutnu.Repo;
using Wutnu.Infrastructure.Filters;
using Wutnu.Web.api;

namespace Wutnu
{
    public class MvcApplication : HttpApplication
    {
        protected async void Application_Start()
        {
            try
            {
                //Registration
                ControllerBuilder.Current.DefaultNamespaces.Add("Wutnu.Controllers");
                AreaRegistration.RegisterAllAreas();

                FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
                GlobalConfiguration.Configure(WebApiConfig.Register);
                RouteConfig.RegisterRoutes(RouteTable.Routes);
                BundleConfig.RegisterBundles(BundleTable.Bundles);
                AntiForgeryConfig.UniqueClaimTypeIdentifier = CustomClaimTypes.ObjectIdentifier;

                //IoC
                var builder = new ContainerBuilder();
                builder.Register(c => new WutNuContext())
                    .AsSelf()
                    .InstancePerRequest();
                builder.Register(c => new WutCache(c.Resolve<WutNuContext>()))
                    .AsSelf()
                    .InstancePerRequest();

                builder.RegisterControllers(typeof(MvcApplication).Assembly);
                builder.RegisterApiControllers(typeof(MvcApplication).Assembly);

                var container = builder.Build();
                DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
                GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);

                //Settings
                Settings.Setup(ConfigurationManager.AppSettings);

                WutStorage.ALLOWED_CORS_ORIGINS = new List<string> { ConfigurationManager.AppSettings["ida:RedirectUri"] };

                var ConfigStorageCors = Convert.ToBoolean(ConfigurationManager.AppSettings["ConfigStorageCors"]);
                bool forceConfig = true;

                if (ConfigStorageCors)
                {
                    WutStorage.ConfigureCors(forceConfig);
                }

                Utils.ApplicationName = "Wut?";
                Cache.RedisConnectionString = ConfigurationManager.AppSettings["RedisConnection"];
                Cache.RedisUrlDBNum = Convert.ToInt32(ConfigurationManager.AppSettings["RedisUrlDBNum"]);
                Cache.RedisUserDBNum = Convert.ToInt32(ConfigurationManager.AppSettings["RedisUserDBNum"]);

                AADGraph.GraphToken = ConfigurationManager.AppSettings["B2BGraphKey"];
                AADGraph.ClientId = ConfigurationManager.AppSettings["ida:ClientIdB2B"];
                AADGraph.TenantName = ConfigurationManager.AppSettings["ida:TenantB2B"];
                AADGraph.TenantId = ConfigurationManager.AppSettings["ida:TenantIdB2B"];
                AADGraph.LoadGroups();

                //VPP
                System.Web.Hosting.HostingEnvironment.RegisterVirtualPathProvider(new WutVirtualPathProvider());
            }
            catch (Exception ex)
            {
                Logging.WriteToAppLog("Error during application startup", System.Diagnostics.EventLogEntryType.Error, ex);
            }
        }

        /// <summary>
        /// Catchall handler when errors are thrown outside the MVC pipeline
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Application_Error(object sender, EventArgs e)
        {
            var ex = Server.GetLastError();
            var contextBase = new HttpContextWrapper(Context);
            try
            {
                if ((ex as HttpException).GetHttpCode() == 404) {
                    var s = "~/Home/Redir" + contextBase.Request.FilePath;
                    contextBase.RewritePath(s, false);
                    contextBase.Server.TransferRequest(s);
                }
            }
            catch {}

            if (Context.Items["ErrorID"] != null)
                return;  //this one has already been handled in one of the MVC error filters

            if (ex.InnerException != null)
                ex = ex.InnerException;

            Server.ClearError();
            if (ex == null) return;
            var code = (ex is HttpException) ? (ex as HttpException).GetHttpCode() : 500;

            var bAjax = IsAjaxRequest();
            var sMessage = (bAjax) ? "AJAX call error" : "";
            var eid = Logging.WriteDebugInfoToErrorLog(sMessage, ex);
            Context.Items.Add("ErrorID", eid);  //to keep us from doing this again in the same call

            Response.Clear();

            if (bAjax)
            {
                //this is a json call; tryskip will return our IDs in response.write, 500 will throw in jquery
                Response.TrySkipIisCustomErrors = true;
                Response.StatusCode = 500;
                Response.StatusDescription = String.Format("{0} Application Error", Utils.ApplicationName);
                Response.ContentType = "application/json";
                Response.Write(JsonConvert.SerializeObject(new ErrResponsePoco { DbErrorId = eid }));
                Response.End();
            }
            else
            {
                try
                {
                    SiteUtils.ReturnViaCode(contextBase, code);
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch (Exception) { }
            }
        }
        /// <summary>
        /// Needed when working with raw Request (HttpRequestBase has this method)
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        private bool IsAjaxRequest()
        {
            if (Request == null)
            {
                // ReSharper disable once NotResolvedInText
                throw new ArgumentNullException(paramName: "HttpRequest");
            }

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            return (Request["X-Requested-With"] == "XMLHttpRequest") || ((Request.Headers != null) && (Request.Headers["X-Requested-With"] == "XMLHttpRequest"));
        }

    }
}
