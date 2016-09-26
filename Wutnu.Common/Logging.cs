using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Http.Filters;
using Wutnu.Common.ErrorMgr;
using Wutnu.Data;

namespace Wutnu.Common
{
    public static class Logging
    {
        public static bool AlertsEnabled { get; set; }
        public static string AlertRecipients { get; set; }
        public static string AppServer { get; set; }

        /// <summary>
        /// For troubleshooting: write a "Debugging Message" to the SQL Local Error Logs. Will not include exception details nor the caller's environment.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static string WriteMessageToErrorLog(string message, HttpContextBase httpContext)
        {
            var entities = httpContext.GetOwinContext().Get<WutNuContext>("WutNuContext");
            return WriteDebugInfoToErrorLog(message, new Exception("Debugging Message"), entities);
        }

        /// <summary>
        /// For troubleshooting: write a "Debugging Message" to the DocNet SQL Local Error Logs. Will not include exception details nor the caller's environment.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="entities"></param>
        /// <returns>ErrorID (string)</returns>
        public static string WriteMessageToErrorLog(string message, WutNuContext entities)
        {
            return WriteDebugInfoToErrorLog(message, new Exception("Debugging Message"), entities);
        }
        /// <summary>
        /// Log an exception in the Error table. Will include exception details only. Logger will instantiate an instance of DocNetEntities and then will dispose.
        /// Optionally include user comments.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        /// <param name="userComment"></param>
        /// <returns>ErrorID (string)</returns>
        public static string WriteDebugInfoToErrorLog(string message, Exception ex, string userComment = "")
        {
            var entities = HttpContext.Current.GetOwinContext().Get<WutNuContext>("WutNuContext");
            return WriteDebugInfoToErrorLog(message, ex, entities, null, userComment);
        }

        /// <summary>
        /// Log an exception in the Error table. Will include exception details only.
        /// Optionally include user comments.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        /// <param name="entities"></param>
        /// <param name="userComment"></param>
        /// <returns>ErrorID (string)</returns>
        public static string WriteDebugInfoToErrorLog(string message, Exception ex, WutNuContext entities, string userComment = "")
        {
            return WriteDebugInfoToErrorLog(message, ex, entities, null, userComment);
        }

        /// <summary>
        /// Log an exception in the Error table. Includes all details from the MVC ExceptionContext filter.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="filterContext"></param>
        /// <returns>ErrorID (string)</returns>
        public static string WriteDebugInfoToErrorLog(string message, ExceptionContext filterContext)
        {
            return WriteDebugInfoToErrorLog(message, filterContext.Exception,
                filterContext.HttpContext.GetOwinContext().Get<WutNuContext>("WutNuContext"), filterContext.HttpContext);
        }

        ///// <summary>
        ///// Log an exception in the Error table. Includes all details from the WebAPI ActionContext filter.
        ///// </summary>
        ///// <param name="message"></param>
        ///// <param name="actionContext"></param>
        ///// <param name="ctx"></param>
        ///// <returns>ErrorID (string)</returns>
        //public static string WriteDebugInfoToErrorLog(string message, HttpActionExecutedContext actionContext, HttpContextBase ctx)
        //{
        //    return WriteDebugInfoToErrorLog(message, actionContext.Exception, ctx, ctx.GetOwinContext().Get<WutNuContext>("WutNuContext"));
        //}

        /// <summary>
        /// Log an exception in the Error table. Includes exception details and user environment.
        /// Optionally include user comments.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        /// <param name="ctx"></param>
        /// <param name="entities"></param>
        /// <param name="userComment"></param>
        /// <returns>ErrorID (string)</returns>
        public static string WriteDebugInfoToErrorLog(string message, Exception ex, WutNuContext entities, HttpContextBase ctx, string userComment = "")
        {
            using (IErrorMgr emgr = new ErrorMgr.ErrorMgr(entities, ctx))
            {
                var res = emgr.InsertError(ex, message, userComment);

                //if (AlertsEnabled)
                //{
                //    MailSender.SendMessage(AlertRecipients, "Wutnu Application Log Error", FormatAlertMessage(message, (ex == null) ? "Unknown" : ex.Source));
                //}

                return res.DbErrorId;
            }
        }

        /// <summary>
        /// Writes an error entry to the Application log, Application Source. This is a fallback error writing mechanism.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="errorType">Type of error.</param>
        /// <param name="ex">Original exception (optional)</param>
        public static void WriteToAppLog(string message, EventLogEntryType errorType, Exception ex = null)
        {
            if (ex != null)
            {
                message += message + " (original error: " + ex.Source + "/" + ex.Message + "\r\nStack Trace: " +
                                ex.StackTrace + ")";
                if (ex.InnerException != null)
                {
                    message += "\r\nInner Exception: " + ex.GetBaseException();
                }
            }
            EventLog.WriteEntry("Application", message, errorType, 0);

            //if (AlertsEnabled)
            //{
            //    SendMessageToAlertRecipients("Wutnu Application Log Error", FormatAlertMessage(message, (ex == null) ? "Unknown" : ex.Source));
            //}
        }

        //public static void SendMessageToAlertRecipients(string subject, string message)
        //{
        //    MailSender.SendMessage(AlertRecipients, subject, message);
        //}

        #region helpers
        private static string FormatAlertMessage(string message, string source)
        {
            var res = new StringBuilder();
            res.AppendFormat("<p>The following error occured at {0} (server time):</p>", DateTime.Now);
            res.AppendLine("    <blockquote>");
            res.AppendFormat("      Message: {0}<br>", message);
            res.AppendFormat("      Source: {0}<br>", source);
            res.AppendLine("    </blockquote>");
            res.AppendFormat("<p>View error details at {0}/ErrorLog</p>", AppServer);
            return Utils.GetHtmlMessageWrapper("Application Alert", res.ToString());
        }
        #endregion
    }
}
