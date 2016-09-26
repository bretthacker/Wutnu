using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Wutnu.Common;
using Wutnu.Data;
using Wutnu.Data.Models;
using Wutnu.Infrastructure;
using System.IdentityModel.Claims;
using Wutnu.Web.Repo;

namespace Wutnu.Web.api
{
    [Authorize]
    public class ReportController : BaseApiController
    {
        ReportRepo _repo;

        public ReportController(WutCache cache, WutNuContext context)
            :base(cache, context)
        {
            _repo = new ReportRepo(context);
        }

        public IEnumerable<Report> GetCustomReports()
        {
            var reportIds = new int[0];
            var extClaimsStr = User.Identity.GetClaim(CustomClaimTypes.ExtClaims);
            if (extClaimsStr!=null)
            {
                var extClaims = JsonConvert.DeserializeObject<ExtClaimsModel>(extClaimsStr);
                reportIds = extClaims.ReportIds;
            }
            return Wutcontext.Reports.Where(r => reportIds.Contains(r.ReportId)).ToList();
        }

        public IEnumerable<Report> GetReports()
        {
            return _repo.GetReportList();
        }
    }
}
