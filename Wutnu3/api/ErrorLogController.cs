using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Wutnu.Data;
using Wutnu.Infrastructure;
using Wutnu.Common.ErrorMgr;

namespace Wutnu.api
{
    public class Err
    {
        public int ErrorId { get; set; }
        public string Status { get; set; }
        public DateTime DeleteBefore { get; set; }
    }

    [Authorize]
    public class ErrorLogController : BaseApiController
    {
        private ErrorItemBL _err;

        public ErrorLogController(WutCache cache, WutNuContext models)
        : base(cache, models)
        {
            _err = new ErrorItemBL(WebContextBase, Wutcontext);
        }

        public ErrorPoco GetErrorItem(int id)
        {
            return _err.GetErrorItem(id);
        }

        public IEnumerable<ErrorPoco> GetErrorItems(int count)
        {
            return _err.GetErrorItems(count).ToList();
        }

        public IEnumerable<ErrorPoco> FindErrorItems(string search)
        {
            return _err.FindErrorItems(search).ToList();
        }

        public IEnumerable<ErrorPoco> GetErrorItemsByStatus(string status)
        {
            return _err.GetErrorItemsByStatus(status).ToList();
        }

        public IEnumerable<ErrorPoco> GetMatchingErrorItems(int id)
        {
            return _err.GetMatchingErrorItems(id).ToList();
        }

        [HttpDelete]
        public ErrorItemsPoco DeleteMatchingErrorItems(Err err)
        {
            var res = new ErrorItemsPoco();
            var count = _err.DeleteMatchingErrorItems(err.ErrorId);
            res.RecordCount = count;
            res.ErrorItems = _err.GetErrorItems();
            return res;
        }
        [HttpDelete]
        public ErrorItemsPoco DeleteErrorItem(Err err)
        {
            var res = new ErrorItemsPoco();
            var count = _err.DeleteErrorItem(err.ErrorId);
            res.RecordCount = count;
            res.ErrorItems = _err.GetErrorItems();
            return res;
        }
        [HttpDelete]
        public ErrorItemsPoco DeleteErrorItemsBeforeDate(Err err)
        {
            var res = new ErrorItemsPoco();
            var count = _err.DeleteErrorItemsBeforeDate(err.DeleteBefore);
            res.RecordCount = count;
            res.ErrorItems = _err.GetErrorItems();
            return res;
        }

        [HttpPost]
        public IEnumerable<ErrorPoco> UpdateErrorItemStatus(Err err)
        {
            _err.UpdateErrorItemStatus(err.ErrorId, err.Status);
            return _err.GetErrorItems().ToList();
        }

        [HttpPost]
        public IEnumerable<ErrorPoco> UpdateAllMatchingItemsStatus(Err err)
        {
            _err.UpdateStatusForMatchingItems(err.ErrorId, err.Status);
            return _err.GetErrorItems().ToList();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _err != null)
            {
                _err = null;
            }
            base.Dispose(disposing);
        }
    }
}
