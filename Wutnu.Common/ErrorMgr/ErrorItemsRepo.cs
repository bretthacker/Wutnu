using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Wutnu.Data;

namespace Wutnu.Common.ErrorMgr
{
    public class ErrorItemsRepo : GenericDocNetRepo<ErrorLog>, IErrorItemsRepo
    {
        private readonly WutNuContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorItemsRepo" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public ErrorItemsRepo(WutNuContext context)
            : base(context)
        {
            _context = context;
        }
        public ErrorLog InsertErrorItem(ErrorLog item)
        {
            var res = _context.ErrorLogs.Add(item);
            _context.SaveChanges();
            return res;
        }
        public ErrorLog GetErrorItem(int id)
        {
            return _context.ErrorLogs.Single(e => e.ErrorId == id);
        }
        public IEnumerable<ErrorLog> GetErrorItems(int count = 100)
        {
            return _context.ErrorLogs.OrderByDescending(e => e.ErrorDate).Take(count).ToList();
        }
        public IEnumerable<ErrorLog> FindErrorItems(string search)
        {
            return
                _context.ErrorLogs.Where(
                    e => e.ErrorMessage.Contains(search) || e.ErrorSource.Contains(search))
                        .OrderByDescending(e => e.ErrorDate)
                        .ToList();
        }
        public IEnumerable<ErrorLog> GetMatchingErrorItems(int id)
        {
            var source = _context.ErrorLogs.Single(e => e.ErrorId == id);
            return
                _context.ErrorLogs.Where(
                    e => e.ErrorSource == source.ErrorSource && e.ErrorMessage == source.ErrorMessage).OrderByDescending(e => e.ErrorDate).ToList();
        }
        public IEnumerable<ErrorLog> UpdateStatusForMatchingItems(int id, ErrorItemStatus status)
        {
            var coll = GetMatchingErrorItems(id).ToList();
            coll.ForEach(e => e.Status = status.ToString());
            coll.ForEach(e => UpdateErrorItem(e));
            _context.SaveChanges();
            return coll.OrderByDescending(e => e.ErrorDate).ToList();
        } 
        public int DeleteMatchingErrorItems(int id)
        {
            GetMatchingErrorItems(id).ToList().ForEach(e => _context.ErrorLogs.Remove(e));
            return _context.SaveChanges();
        }
        public int DeleteErrorItem(int id)
        {
            _context.ErrorLogs.Remove(GetErrorItem(id));
            return _context.SaveChanges();
        }

        public IEnumerable<ErrorLog> GetErrorItemsByStatus(ErrorItemStatus status)
        {
            return _context.ErrorLogs.Where(e => e.Status == status.ToString());
        }
        public ErrorLog UpdateErrorItem(ErrorLog item)
        {
            _context.ErrorLogs.Attach(item);
            _context.Entry(item).State = EntityState.Modified;
            _context.SaveChanges();
            return item;
        }
        /// <summary>
        /// Delete all ErrorItems prior to passed-in date
        /// </summary>
        /// <param name="deleteBefore">Date before which all items should be deleted</param>
        /// <returns>int (count of items deleted)</returns>
        public int DeleteErrorItemsBeforeDate(DateTime deleteBefore)
        {

            _context.ErrorLogs.Where(e => e.ErrorDate < deleteBefore)
                    .ToList()
                    .ForEach(e => _context.ErrorLogs.Remove(e));
            //returning number of items deleted
            return _context.SaveChanges();
        }
    }
}
