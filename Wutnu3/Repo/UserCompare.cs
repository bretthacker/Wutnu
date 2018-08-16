using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Wutnu.Data.Models;

namespace Wutnu.Web.Repo
{
    public class UserCompare : IEqualityComparer<AssignedUserPoco>
    {
        public bool Equals(AssignedUserPoco u1, AssignedUserPoco u2)
        {
            return (u1.PrimaryEmail == u2.PrimaryEmail);
        }

        public int GetHashCode(AssignedUserPoco user)
        {
            //Check whether the object is null
            if (object.ReferenceEquals(user, null)) return 0;

            //Get hash code for the Name field if it is not null.
            int hashEmail = user.PrimaryEmail == null ? 0 : user.PrimaryEmail.GetHashCode();
            return hashEmail;
        }
    }
}