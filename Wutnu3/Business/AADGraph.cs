using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Azure.ActiveDirectory;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.Azure.ActiveDirectory.GraphClient.Extensions;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Wutnu.Web.Business;
using System.Security.Principal;
using Wutnu.Common;
using System.IdentityModel.Claims;

namespace Wutnu.Business
{
    public static class AADGraph
    {
        public static string GraphToken { get; set; }
        public static string ClientId { get; set; }
        private static string _tenantName;

        public static string TenantName
        {
            get
            {
                return _tenantName;
            }
            set
            {
                Guid x;
                if (!Guid.TryParse(value, out x))
                {
                    TenantId = GetAADTenantId(value);
                }
                else
                {
                    TenantId = value;
                }
                _tenantName = value;
            }
        }
        /// <summary>
        /// AAD app registration host tenant
        /// </summary>
        public static string TenantId { get; set; }

        public static List<Group> GroupList { get; set; }
        public static IEnumerable<IDirectoryObject> AdminUsers { get; set; }

        public static string AdminGroupId { get; set; }

        private static string authString;
        const string resAzureGraphAPI = "https://graph.windows.net";

        public async static Task LoadGroups()
        {
            authString = string.Format("https://login.microsoftonline.com/{0}/oauth2/authorize", _tenantName);

            GroupList = new List<Group>();

            ActiveDirectoryClient client = GetClient();
            await GetAdmins(client);
            IPagedCollection<IGroup> coll = await client.Groups.ExecuteAsync();
            while (coll != null)
            {
                List<IGroup> groups = coll.CurrentPage.ToList();
                foreach (IGroup group in groups)
                {
                    GroupList.Add((Group)group);
                }
                coll = coll.GetNextPageAsync().Result;
            }
        }

        public async static Task GetAdmins(ActiveDirectoryClient client)
        {
            if (AdminGroupId == "")
            {
                AdminUsers = null;
                return;
            }
            var members = await client.Groups.GetByObjectId(AdminGroupId).Members.ExecuteAsync();
            AdminUsers = members.CurrentPage.ToList();
        }

        public static IUser GetUser(string objectId)
        {
            User user = null;
            try
            {
                ActiveDirectoryClient client = GetClient();
                user = (User)client.Users.GetByObjectId(objectId).ExecuteAsync().Result;
                return user;
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting user info from Graph", ex);
            }
        }

        private static ActiveDirectoryClient GetClient()
        {
            Uri servicePointUri = new Uri("https://graph.windows.net");

            Uri serviceRoot = new Uri(servicePointUri, _tenantName);

            ActiveDirectoryClient activeDirectoryClient = new ActiveDirectoryClient(serviceRoot, async () => await GetAppTokenAsync());

            return activeDirectoryClient;
        }

        private static async Task<string> GetAppTokenAsync()
        {
            // Instantiate an AuthenticationContext for my directory (see authString above).
            AuthenticationContext authenticationContext = new AuthenticationContext(authString, false);

            // Create a ClientCredential that will be used for authentication.
            // This is where the Client ID and Key/Secret from the Azure Management Portal is used.
            ClientCredential clientCred = new ClientCredential(ClientId, GraphToken);

            // Acquire an access token from Azure AD to access the Azure AD Graph (the resource)
            // using the Client ID and Key/Secret as credentials.
            AuthenticationResult authenticationResult = await authenticationContext.AcquireTokenAsync(resAzureGraphAPI, clientCred);

            // Return the access token.
            return authenticationResult.AccessToken;
        }
        public static string GetAADTenantId(string domainName)
        {
            var uri = string.Format("https://login.windows.net/{0}/.well-known/openid-configuration", domainName);
            string res = "";
            using (var web = new WebClient())
            {
                try
                {
                    res = web.DownloadString(uri);
                }
                catch (WebException exception)
                {
                    using (var reader = new StreamReader(exception.Response.GetResponseStream()))
                    {
                        res = reader.ReadToEnd();
                    }
                }
                var info = JsonConvert.DeserializeObject<OIDConfigResponse>(res);
                res = new Uri(info.Issuer).Segments[1].TrimEnd('/');
            }
            return res;
        }

        /// <summary>
        /// Look up AAD Tenant ID from UPN suffix
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        public static string GetUserTenantId(IIdentity identity)
        {
            if (identity.HasClaim(CustomClaimTypes.TenantId))
            {
                return identity.GetClaim(CustomClaimTypes.TenantId);
            }

            var str = identity.GetClaim(ClaimTypes.Upn);
            if (str == null)
                str = identity.GetClaim(ClaimTypes.Email);
            var domain = str.Split('@')[1];
            return GetAADTenantId(domain);
        }
    }
}