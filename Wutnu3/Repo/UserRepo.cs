using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Wutnu.Data.Models;
using Wutnu.Data;
using Wutnu.Common;

namespace Wutnu.Web.Repo
{
    public class UserRepo
    {
        WutNuContext _context;

        public UserRepo(WutNuContext context)
        {
            _context = context;
        }

        public UserPoco GetUser(string email)
        {
            return UserPoco.UserToUserPoco(_context.Users.SingleOrDefault(u => u.PrimaryEmail == email));
        }
        public UserPoco GetUser(int userId)
        {
            return UserPoco.UserToUserPoco(_context.Users.SingleOrDefault(u => u.UserId==userId));
        }
        public UserPoco UpdateUser(UserPoco user)
        {
            try
            {
                var u = _context.Users.SingleOrDefault(usr => usr.UserId == user.UserId);
                u.ApiKey = user.ApiKey;
                _context.SaveChanges();

                return user;
            }
            catch (Exception ex)
            {
                Logging.WriteDebugInfoToErrorLog("Error updating user", ex);
                return null;
            }
        }
        public IEnumerable<AssignedUserPoco> DeleteAssignedUser(int ownerId, UserPoco user)
        {
            var del = _context.UserAssignments.FirstOrDefault(u => u.UserEmail == user.PrimaryEmail && u.WutLink.UserId == ownerId);
            _context.UserAssignments.Remove(del);
            _context.SaveChanges();
            return GetAssignedUsers(ownerId);
        }

        public IEnumerable<AssignedUserPoco> GetAssignedUsers(int ownerId)
        {
            var res = _context.UserAssignments
                .Include("WutLinks")
                .Include("WutLinks.Users")
                .Include("WutLinks.Users.WutLinks")
                .Where(u => u.WutLink.UserId == ownerId)
                .Select(u => new AssignedUserPoco
                {
                    UserId = u.UserId,
                    PrimaryEmail = u.UserEmail,
                    AssignedLinks = u.WutLink.User.WutLinks
                        .Where(l => l.UserAssignments.Any(m => m.UserEmail == u.UserEmail))
                        .Select(n => new WutLinkPoco
                        {
                            RealUrl = n.RealUrl,
                            IsAzureBlob = n.IsAzureBlob,
                            Comments = n.Comments,
                            CreateDate = n.CreateDate
                        })
                    }
                )
                .ToList().Distinct(new UserCompare());
            
            return (res as IEnumerable<AssignedUserPoco>);
        }
    }
}