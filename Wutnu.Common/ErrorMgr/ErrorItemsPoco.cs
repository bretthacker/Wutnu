using System.Collections.Generic;

namespace Wutnu.Common.ErrorMgr
{
    public class ErrorItemsPoco
    {
        public int RecordCount { get; set; }
        public IEnumerable<ErrorPoco> ErrorItems { get; set; }
    }
}
