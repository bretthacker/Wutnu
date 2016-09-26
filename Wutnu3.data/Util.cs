using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wutnu.Data
{
    public static class Util
    {
        public static string BuildEntityConnectionStringFromAppSettings(string nameOfConnectionString, string modelName = null)
        {
            var shortConnectionString = GetConnectionStringByName(nameOfConnectionString);
            modelName = (modelName ?? nameOfConnectionString);

            // Specify the provider name, server and database. 
            string providerName = "System.Data.SqlClient";

            // Initialize the connection string builder for the 
            // underlying provider taking the short connection string.
            SqlConnectionStringBuilder sqlBuilder =
                new SqlConnectionStringBuilder(shortConnectionString);

            // Build the SqlConnection connection string. 
            string providerString = sqlBuilder.ToString();

            // Initialize the EntityConnectionStringBuilder.
            EntityConnectionStringBuilder entityBuilder = new EntityConnectionStringBuilder();

            //Set the provider name.
            entityBuilder.Provider = providerName;

            // Set the provider-specific connection string.
            entityBuilder.ProviderConnectionString = providerString;

            // Set the Metadata location.
            entityBuilder.Metadata = string.Format("res://*/{0}.csdl|res://*/{0}.ssdl|res://*/{0}.msl", modelName);

            return entityBuilder.ToString();
        }

        private static string GetConnectionStringByName(string nameOfConnectionString)
        {
            return ConfigurationManager.ConnectionStrings[nameOfConnectionString].ToString();
        }
    }
}
