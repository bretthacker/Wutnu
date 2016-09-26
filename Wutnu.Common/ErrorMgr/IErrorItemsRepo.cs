using System;
using System.Collections.Generic;
using Wutnu.Data;

namespace Wutnu.Common.ErrorMgr
{
    public interface IErrorItemsRepo
    {
        ErrorLog InsertErrorItem(ErrorLog item);
        ErrorLog GetErrorItem(int id);
        IEnumerable<ErrorLog> GetErrorItems(int count = 100);
        IEnumerable<ErrorLog> FindErrorItems(string search);
        IEnumerable<ErrorLog> GetMatchingErrorItems(int id);
        int DeleteMatchingErrorItems(int id);
        int DeleteErrorItem(int id);
        IEnumerable<ErrorLog> UpdateStatusForMatchingItems(int id, ErrorItemStatus status);
        IEnumerable<ErrorLog> GetErrorItemsByStatus(ErrorItemStatus status);
        ErrorLog UpdateErrorItem(ErrorLog item);
        int DeleteErrorItemsBeforeDate(DateTime deleteBefore);
    }
    public enum ErrorItemStatus
    {
        New,
        Assigned,
        Resolved,
        WillNotFix,
        Closed
    }
}
