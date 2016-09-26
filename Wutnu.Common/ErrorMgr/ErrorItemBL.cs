using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Wutnu.Common.Helpers;
using Wutnu.Data;

namespace Wutnu.Common.ErrorMgr
{
    public class ErrorItemBL: BaseBusinessLayer
    {
        private readonly IErrorItemsRepo _repo;

        public ErrorItemBL(HttpContextBase httpContext, WutNuContext io, bool disposeEntity=true)
            : base(httpContext, io)
        {
            _repo = new ErrorItemsRepo(io);
        }

        public ErrorPoco GetErrorItem(int id)
        {
            return GetErrorItemPoco(_repo.GetErrorItem(id));
        }

        public IEnumerable<ErrorPoco> GetErrorItems(int count = 100)
        {
            return _repo.GetErrorItems(count).Select(GetErrorItemPoco);
        }

        public IEnumerable<ErrorPoco> FindErrorItems(string search)
        {
            return _repo.FindErrorItems(search).Select(GetErrorItemPoco);
        }

        public IEnumerable<ErrorPoco> GetMatchingErrorItems(int id)
        {
            return _repo.GetMatchingErrorItems(id).Select(GetErrorItemPoco);
        }

        public int DeleteMatchingErrorItems(int id)
        {
            return _repo.DeleteMatchingErrorItems(id);
        }
        public int DeleteErrorItem(int id)
        {
            return _repo.DeleteErrorItem(id);
        }

        public IEnumerable<ErrorPoco> GetErrorItemsByStatus(string statusString)
        {
            ErrorItemStatus status;
            Enum.TryParse(statusString, true, out status);

            return _repo.GetErrorItemsByStatus(status).Select(GetErrorItemPoco);
        }

        public ErrorPoco UpdateErrorItemUserComment(int id, string comment)
        {
            var item = _repo.GetErrorItem(id);
            item.UserComment = comment;
            return GetErrorItemPoco(_repo.UpdateErrorItem(item));
        }

        public ErrorPoco UpdateErrorItemStatus(int id, string status)
        {
            var item = _repo.GetErrorItem(id);
            ErrorItemStatus oStatus;
            if (!Enum.TryParse(status, true, out oStatus))
                throw new Exception("Status \"" + status +
                                    "\" is not an acceptable status value for an ErrorItem updaterr.");

            //we won't use the oStatus object, we just needed it for the parse test; there may be a better way
            item.Status = status;
            return GetErrorItemPoco(_repo.UpdateErrorItem(item));
            //if we're here, the status didn't match
        }

        public IEnumerable<ErrorPoco> UpdateStatusForMatchingItems(int id, string status)
        {
            ErrorItemStatus oStatus;
            if (Enum.TryParse(status, true, out oStatus))
            {
                return _repo.UpdateStatusForMatchingItems(id, oStatus).Select(GetErrorItemPoco);
            }
            //if we're here, the status didn't match
            throw new Exception("Status \"" + status + "\" is not an acceptable status value for an ErrorItem updaterr.");
        }

        /// <summary>
        /// Delete all error items created before the passed-in date
        /// </summary>
        /// <param name="deleteBefore">The date before which items will be deleted</param>
        /// <returns>int - the number of items deleted</returns>
        public int DeleteErrorItemsBeforeDate(DateTime deleteBefore)
        {
            return _repo.DeleteErrorItemsBeforeDate(deleteBefore);
        }

        #region helpers
        private static ErrorPoco GetErrorItemPoco(ErrorLog err)
        {
            return new ErrorPoco
            {
                ErrorDate = err.ErrorDate,
                ErrorId = err.ErrorId,
                ErrorMessage = err.ErrorMessage,
                ErrorSource = err.ErrorSource,
                InnerExceptionMessage = err.InnerExceptionMessage,
                InnerExceptionSource = err.InnerExceptionSource,
                IPAddress = err.IPAddress,
                PostData = err.PostData,
                QSData = err.QSData,
                UserComment = err.UserComment,
                Referrer = err.Referrer,
                StackTrace = err.StackTrace,
                Status = err.Status,
                UserAgent = err.UserAgent,
                ValidationErrors = err.ValidationErrors
            };
        }
        #endregion
    }
}
