using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web;
using Wutnu.Data;

namespace Wutnu.Common.ErrorMgr
{
    public class ErrorMgr: IErrorMgr
    {
        private readonly IErrorMgr _mgr;
        public ErrorMgr(WutNuContext entities, HttpContextBase ctx)
        {
            _mgr = new ErrorMgrDb(entities, ctx);
            /*
            switch (destination)
            {
                case ErrorDest.Sql:
                    _mgr = new ErrorMgrDb(entities, ctx);
                    break;
            }
            */
        }

        public IEnumerable<ErrItemPoco> GetErrorItems(int count=100)
        {
            return _mgr.GetErrorItems(count);
        }
        public ErrItemPoco ReadError(string id)
        {
            return _mgr.ReadError(id);
        }
        public bool SaveError(ErrItemPoco eo)
        {
            return _mgr.SaveError(eo);
        }
        public ErrResponsePoco InsertError(Exception err, string message = "", string userComment="")
        {
            return _mgr.InsertError(err, message, userComment);
        }

        /// <summary>
        /// Writes an error entry to the Application log, Application Source. This is a fallback error writing mechanism.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="errorType">Type of error.</param>
        public void WriteToAppLog(string message, EventLogEntryType errorType)
        {
            Logging.WriteToAppLog(message, errorType);
        }

        public void Dispose()
        {
            _mgr.Dispose();
        }

    }
    public class ErrItemPoco
    {
        public string BlobId { get; set; }
        public int Id { get; set; }
        public DateTime ErrorDate { get; set; }
        public string Status { get; set; }
        public string ErrorSource { get; set; }
        public string ErrorMessage { get; set; }
        public string StackTrace { get; set; }
        public string InnerException { get; set; }
        public string Message { get; set; }
        public string UserAgent { get; set; }
        public string IPAddress { get; set; }
        public string UserId { get; set; }
        public string URI { get; set; }
        public string SiteURL { get; set; }
        public string Referrer { get; set; }
        public string PostData { get; set; }
        public string QSData { get; set; }
        public string UserComment { get; set; }
    }
    public class ErrResponsePoco
    {
        public string DbErrorId { get; set; }
        public string ErrorTitle { get; set; }
        public string ErrorMessage { get; set; }
    }
    public enum ErrorDest
    {
        Sql
    }
}
