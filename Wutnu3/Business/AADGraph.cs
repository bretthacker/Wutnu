using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Azure.ActiveDirectory;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.Azure.ActiveDirectory.GraphClient.Extensions;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Wutnu.Business
{
    public static class AADGraph
    {
        public static string GraphToken { get; set; }
        public static string ClientId { get; set; }
        public static string TenantId { get; set; }
        public static string TenantName { get; set; }

        public static List<Group> GroupList { get; set; }

        private static string authString;
        const string resAzureGraphAPI = "https://graph.windows.net";

        public async static void LoadGroups()
        {
            authString = string.Format("https://login.microsoftonline.com/{0}/oauth2/authorize", TenantId);

            GroupList = new List<Group>();

            ActiveDirectoryClient client = GetClient();
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
                throw;
            }
        }

        private static ActiveDirectoryClient GetClient()
        {
            Uri servicePointUri = new Uri("https://graph.windows.net");

            Uri serviceRoot = new Uri(servicePointUri, TenantId);

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
    }
}