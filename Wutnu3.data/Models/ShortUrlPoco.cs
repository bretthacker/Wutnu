using System;
using System.Collections.Generic;
using System.Linq;

namespace Wutnu.Data
{
    public class WutLinkPoco
    {
        public int LinkId { get; set; }

        public string ShortUrl { get; set; }

        public int? UserId { get; set; }

        public DateTime CreateDate { get; set; }

        public string CreatedByIp { get; set; }

        public bool IsProtected { get; set; }

        public bool IsAzureBlob { get; set; }

        public string RealUrl { get; set; }

        public string Comments { get; set; }

        public string UserEmails { get; set; }

        public bool UserAuthenticated { get; set; }

        public List<string> UserEmailColl { get; set; }

        public WutLinkPoco()
        {
            UserEmailColl = new List<string>();
        }

        public static WutLinkPoco GetPocoFromObject(WutLink obj)
        {
            var emails = (obj.UserAssignments.Count == 0) ? "" : string.Join(", ", obj.UserAssignments.Select(o => {
                    return (o.User != null) ? o.User.PrimaryEmail : o.UserEmail;
                }
            ));
            
            var res = new WutLinkPoco
            {
                LinkId=obj.WutLinkId,
                Comments=obj.Comments,
                CreateDate=obj.CreateDate,
                CreatedByIp=obj.CreatedByIp,
                IsProtected=obj.IsProtected,
                UserId=obj.UserId,
                RealUrl = obj.RealUrl,
                ShortUrl = obj.ShortUrl,
                UserEmails = emails, 
                IsAzureBlob = obj.IsAzureBlob
            };

            res.UserEmailColl.AddRange(obj.UserAssignments.Select(u => u.UserEmail).ToList());

            return res;
        }
    }
}
