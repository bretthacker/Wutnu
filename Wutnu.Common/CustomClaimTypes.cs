using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wutnu.Common
{
    public static class CustomClaimTypes
    {
        public const string UserId = "UserId";
        public const string IdentityProvider = "http://schemas.microsoft.com/identity/claims/identityprovider";
        public const string ExtClaims = "ExtClaims";
        public const string AuthType = "AuthType";
        public const string FullName = "FullName";
        public const string ObjectIdentifier = "http://schemas.microsoft.com/identity/claims/objectidentifier";
        public const string TenantId = "http://schemas.microsoft.com/identity/claims/tenantid";
    }
    public static class WutAuthTypes
    {
        public const string LocalAAD = "OIDC-LocalAAD";
        public const string B2B = "OIDC-B2B";
        public const string B2EMulti = "OIDC-B2EMulti";
        public const string B2C = "OIDC-B2C";
        public const string Unified = "OIDC-B2CUnified";
        public const string Api = "ApiAuth";
    }
    public static class WutRoles
    {
        public const string WutNuAdmins = "SiteAdmin";
    }
}