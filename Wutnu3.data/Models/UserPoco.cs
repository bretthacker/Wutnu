using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wutnu.Data.Models
{
    public class UserPoco
    {
        public int UserId { get; set; }
        public string UserOID { get; set; }
        public string ApiKey { get; set; }
        public string PrimaryEmail { get; set; }

        public static UserPoco UserToUserPoco(User user)
        {
            var res = new UserPoco
            {
                ApiKey = user.ApiKey,
                PrimaryEmail = user.PrimaryEmail,
                UserId = user.UserId,
                UserOID = user.UserOID
            };

            return res;
        }
    }
}
