using System;
using System.Collections.Generic;

namespace Wutnu.Common.ErrorMgr
{
    public interface IErrorMgr: IDisposable
    {
        ErrItemPoco ReadError(string id);
        bool SaveError(ErrItemPoco eo);
        ErrResponsePoco InsertError(Exception err, string message = "", string userComment="");
        IEnumerable<ErrItemPoco> GetErrorItems(int count = 100);
    }
}
