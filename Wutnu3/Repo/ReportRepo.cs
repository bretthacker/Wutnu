using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Wutnu.Data;

namespace Wutnu.Web.Repo
{
    public class ReportRepo
    {
        WutNuContext _context;

        public ReportRepo(WutNuContext context)
        {
            _context = context;
        }

        public IEnumerable<Report> GetReportList()
        {
            return _context.Reports.OrderBy(r => r.ReportName).ToList();
        }
    }
}