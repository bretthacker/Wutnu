using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Wutnu.Business;
using Wutnu.Data;
using Wutnu.Data.Models;
using Wutnu.Infrastructure;
using Wutnu.Web.Repo;

namespace Wutnu.Web.api
{
    [Authorize]
    public class ProfileController : BaseApiController
    {
        UserRepo _repo;

        public ProfileController(WutCache cache, WutNuContext models)
        : base(cache, models)
        {
            _repo = new UserRepo(models);
        }

        [HttpPost]
        public UserPoco ResetApiKey(UserPoco data)
        {
            data.UserId = UserId;
            data.ApiKey = SiteUtils.GenApiKey();

            return _repo.UpdateUser(data);
        }

        public IEnumerable<AssignedUserPoco> GetUsers()
        {
            return _repo.GetAssignedUsers(UserId);
        }

        [HttpPost]
        public IEnumerable<AssignedUserPoco> DeleteUser(UserPoco user)
        {
            return _repo.DeleteAssignedUser(UserId, user);
        }
        public bool IsUserInAAD(string objectId)
        {
            var res = AADGraph.GetUser(objectId);
            return (res != null);
        }
    }
}
