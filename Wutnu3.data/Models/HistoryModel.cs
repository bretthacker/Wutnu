using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wutnu.Data.Models
{
    public class HistoryModel
    {
        public int HistoryId { get; set; }
        public DateTime CallDate { get; set; }
        public string HostIp { get; set; }
        public string RealUrl { get; set; }
        public int WutLinkId { get; set; }
        public string ShortUrl { get; set; }
        public string UserName { get; set; }
    }
}
