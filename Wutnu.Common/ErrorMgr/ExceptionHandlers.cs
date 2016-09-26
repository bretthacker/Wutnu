using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Web.Mvc;
using Wutnu.Common.ErrorMgr;
using Wutnu.Data;

namespace Wutnu.Common.Exceptions
{
    public class HandleAndLogErrorAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            var message = string.Format("Exception     : {0}\n" +
                                        "InnerException: {1}",
                filterContext.Exception,
                filterContext.Exception.InnerException);

            var entities = (filterContext.HttpContext.GetService(typeof(WutNuContext)) as WutNuContext);

            var eid = Logging.WriteDebugInfoToErrorLog(message, filterContext);
            filterContext.HttpContext.Items.Add("ErrorID", eid);
            if (HttpContext.Current.Session!=null) HttpContext.Current.Session["ErrorID"] = eid;
            
            filterContext.ExceptionHandled = true;

            base.OnException(filterContext);

            // Verify if AJAX request
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                // Use partial view in case of AJAX request
                //var result = new PartialViewResult {ViewName = PartialViewName};
                var result = new JsonResult
                {
                    Data = new ErrResponsePoco
                    {
                        DbErrorId = eid
                    }
                };
                filterContext.Result = result;
            }
            else
            {
                filterContext.Result = new ViewResult
                {
                    ViewName = "~/Views/Shared/Error.cshtml"
                };
            }
        }
    }

    public class HandleWebApiException : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionContext)
        {
            var context = new HttpContextWrapper(HttpContext.Current);
            
            actionContext.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            var resex = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            var res = new ErrResponsePoco();
            var entities = (actionContext.Request.GetDependencyScope().GetService(typeof(WutNuContext)) as WutNuContext);
            res.DbErrorId = AddErrorLogItem(actionContext.Exception, context, entities);

            context.ClearError();
            resex.Content = new ObjectContent(typeof(ErrResponsePoco), res, new JsonMediaTypeFormatter());

            throw new HttpResponseException(resex);
        }

        private static string AddErrorLogItem(Exception ex, HttpContextBase htxBase, WutNuContext entities)
        {
            //todo: we don't have OWIN context available here...???

            var eid = Logging.WriteDebugInfoToErrorLog("WebAPI Error", ex, entities, htxBase);
            htxBase.Items.Add("ErrorID", eid);
            return eid;
        }
    }
}