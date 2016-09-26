using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wutnu.Data.Models
{
    public class AssignedUserPoco
    {
        public int? UserId { get; set; }
        public string PrimaryEmail { get; set; }
        public IEnumerable<WutLinkPoco> AssignedLinks { get; set; }
    }
}
