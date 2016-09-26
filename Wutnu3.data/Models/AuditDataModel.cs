using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wutnu.Models
{
    public class AuditDataModel
    {
        public string ShortUrl { get; set; }

        public DateTime CallDate { get; set; }

        public int? UserId { get; set; }

        public string HostIp { get; set; }
    }
}
