using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using Wutnu.Data;

namespace Wutnu.Common.ErrorMgr
{
    public class ErrorMgrDb: IErrorMgr
    {
        private readonly WutNuContext _entities;
        private readonly HttpContextBase _ctx;
        private readonly bool _isNewContext;

        public ErrorMgrDb(WutNuContext entities, HttpContextBase ctx)
        {
            if (entities == null)
            {
                _isNewContext = true;
                entities=new WutNuContext();
            }
            _entities = entities;
            _ctx = ctx;
        }

        public IEnumerable<ErrItemPoco> GetErrorItems(int count=100)
        {
            var repo = new ErrorItemsRepo(_entities);
            return repo.GetErrorItems(count).Select(e => new ErrItemPoco
            {
                ErrorDate = e.ErrorDate,
                ErrorMessage = e.ErrorMessage,
                ErrorSource = e.ErrorSource,
                Id = e.ErrorId,
                InnerException = e.InnerExceptionMessage,
                IPAddress = e.IPAddress,
                PostData = e.PostData,
                QSData = e.QSData,
                Referrer = e.Referrer,
                StackTrace = e.StackTrace,
                Status = e.Status,
                UserAgent = e.UserAgent,
                UserComment = e.UserComment
            }).ToList();
        }

        public ErrItemPoco ReadError(string id)
        {
            var repo = new ErrorItemsRepo(_entities);
            return ConvertDbToPoco(repo.GetErrorItem(Convert.ToInt32(id)));
        }
        private ErrorLog ConvertPocoToDb(ErrItemPoco item)
        {
            return JsonConvert.DeserializeObject<ErrorLog>(JsonConvert.SerializeObject(item));
        }
        private ErrItemPoco ConvertDbToPoco(ErrorLog item)
        {
            return JsonConvert.DeserializeObject<ErrItemPoco>(JsonConvert.SerializeObject(item));
        }
        public bool SaveError(ErrItemPoco eo)
        {
            try
            {
                var dbItem = ConvertPocoToDb(eo);

                var repo = new ErrorItemsRepo(new WutNuContext());
                if (eo.Id > 0)
                {
                    repo.UpdateErrorItem(dbItem);
                }
                else
                {
                    repo.InsertErrorItem(dbItem);
                }
                return true;
            }
            catch (Exception ex)
            {
                Logging.WriteToAppLog("Error saving SiteError object to DB: " + ex.Message, EventLogEntryType.Error);
                return false;
            }
        }
        public ErrResponsePoco InsertErrorItem(ErrorLog item, string message = "")
        {
            var repo = new ErrorItemsRepo(_entities);
            item = repo.InsertErrorItem(item);

            return new ErrResponsePoco
            {
                DbErrorId = item.ErrorId.ToString(CultureInfo.InvariantCulture)
            };
        }
        public ErrResponsePoco InsertError(Exception err, string message = "", string userComment="")
        {
            var item = new ErrorLog
            {
                InnerExceptionMessage = ((err.InnerException != null) ? err.GetBaseException().ToString() : ""),
                Status = "New",
                StackTrace = err.StackTrace ?? "N/A",
                ErrorDate = DateTime.UtcNow,
                ErrorSource = err.Source ?? "N/A",
                ErrorMessage = err.Message,
                UserComment = userComment
            };
            try
            {
                if (item.ErrorMessage.IndexOf("EntityValidationErrors", StringComparison.Ordinal) > 0)
                {
                    var baseException = err.GetBaseException();
                    if (baseException.GetType() == typeof (DbEntityValidationException))
                    {
                        var dbEntityValidationException = (DbEntityValidationException)err.GetBaseException();
                        var valErrString = new StringBuilder();
                        var valErrors = dbEntityValidationException.EntityValidationErrors;
                        valErrors.ToList().ForEach(e => e.ValidationErrors.ToList().ForEach(
                            delegate(DbValidationError er)
                            {
                                var res = String.Format("{0}.{1}: {2}", e.Entry.Entity.ToString(), er.PropertyName,
                                    er.ErrorMessage);
                                if (!valErrString.ToString().Contains(res)) valErrString.AppendLine(res);
                            }));
                        item.ValidationErrors = valErrString.ToString();
                    }
                }

                if (_ctx == null) return InsertErrorItem(item, message);

                item.UserAgent = _ctx.Request.UserAgent;
                item.IPAddress = _ctx.Request.ServerVariables["REMOTE_HOST"];
                item.Referrer = (_ctx.Request.UrlReferrer==null) ? "N/A" : _ctx.Request.UrlReferrer.ToString();
                item.PostData = _ctx.Server.HtmlEncode(_ctx.Request.Form.ToString());
                item.QSData = _ctx.Server.HtmlEncode((_ctx.Request.Url == null) ? "N/A" : _ctx.Request.Url.Query);

                return InsertErrorItem(item, message);
            }
            catch (Exception ex)
            {
                try
                {
                    Logging.WriteToAppLog(err.Message + " (will attempt writing directly)", EventLogEntryType.Error, ex);
                    using (var cx = new WutNuContext())
                    {
                        var repo = new ErrorItemsRepo(cx);
                        item = repo.InsertErrorItem(item);

                        return new ErrResponsePoco
                        {
                            DbErrorId = item.ErrorId.ToString(CultureInfo.InvariantCulture)
                        };
                    }
                }
                catch (Exception ex2)
                {
                    Logging.WriteToAppLog(err.Message + " (2nd attempt with direct entity creation)", EventLogEntryType.Error, ex2);
                    return null;
                }
            }
        }

        public void Dispose()
        {
            if (_isNewContext)
            {
                _entities.Dispose();
            }
        }
    }
}
