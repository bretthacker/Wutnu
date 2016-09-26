using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Azure; // Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Blob; // Namespace for Blob storage types
using System.IO;
using Wutnu.Common;
using System.Text;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using System.Globalization;

namespace Wutnu.Repo
{
    public static class WutStorage
    {
        const CorsHttpMethods ALLOWED_CORS_METHODS = CorsHttpMethods.Delete | CorsHttpMethods.Put | CorsHttpMethods.Get | CorsHttpMethods.Connect | CorsHttpMethods.Head;
        const int ALLOWED_CORS_AGE_DAYS = 5;

        public static List<string> ALLOWED_CORS_ORIGINS;
        public static List<string> ALLOWED_CORS_HEADERS = new List<string> { "x-ms-*", "content-length", "accept", "content-type" };

        public static CloudStorageAccount GetStorageAccount()
        {
            return CloudStorageAccount.Parse(Settings.StorageConnectionString);
        }

        public static CloudBlobContainer GetContainer(string containerName)
        {
            // Parse the connection string and return a reference to the storage account.
            CloudStorageAccount storageAccount = GetStorageAccount();
            
            var client = storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container.
            CloudBlobContainer container = client.GetContainerReference(containerName);

            // Create the container if it doesn't already exist.
            container.CreateIfNotExists();
            //default container is "private"
            return container;
        }

        public static IEnumerable<IListBlobItem> GetBlobs(CloudBlobContainer container)
        {
            return container.ListBlobs(null, true, BlobListingDetails.None);
        }

        public static CloudBlob GetBlob(CloudBlobContainer container, string blobReference)
        {
            return container.GetBlockBlobReference(blobReference);
        }

        public static bool DeleteBlob(CloudBlobContainer container, string blobRef)
        {
            blobRef = blobRef.Replace("/", "");

            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobRef);

            // Delete the blob.
            return blockBlob.DeleteIfExists();
        }

        /// <summary>
        /// https://azure.microsoft.com/en-us/documentation/articles/storage-dotnet-shared-access-signature-part-2/
        /// </summary>
        /// <param name="container"></param>
        /// <param name="blobRef"></param>
        /// <returns></returns>
        public static string GetBlobReadTokenUri(CloudBlobContainer container, string blobRef)
        {
            //Get a reference to a blob within the container.
            CloudBlockBlob blob = container.GetBlockBlobReference(blobRef);

            //Set the expiry time and permissions for the blob.
            //In this case the start time is specified as a few minutes in the past, to mitigate clock skew.
            //The shared access signature will be valid immediately.
            SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy();
            sasConstraints.SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-5);
            sasConstraints.SharedAccessExpiryTime = DateTime.UtcNow.AddHours(24);
            sasConstraints.Permissions = SharedAccessBlobPermissions.Read;

            //Generate the shared access signature on the blob, setting the constraints directly on the signature.
            string sasBlobToken = blob.GetSharedAccessSignature(sasConstraints);

            //Return the URI string for the container, including the SAS token.
            return string.Format(CultureInfo.InvariantCulture, "{0}{1}", blob.Uri, sasBlobToken);
        }

        public static string GetBlobWriteTokenUri(CloudBlobContainer container, string blobRef)
        {
            blobRef = blobRef.Replace("/", "");

            //Get a reference to a blob within the container.
            CloudBlockBlob blob = container.GetBlockBlobReference(blobRef);

            //Set the expiry time and permissions for the blob.
            //In this case the start time is specified as a few minutes in the past, to mitigate clock skew.
            //The shared access signature will be valid immediately.
            SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy();
            sasConstraints.SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-5);
            sasConstraints.SharedAccessExpiryTime = DateTime.UtcNow.AddHours(24);
            sasConstraints.Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.Write;

            //Generate the shared access signature on the blob, setting the constraints directly on the signature.
            string sasBlobToken = blob.GetSharedAccessSignature(sasConstraints);

            //Return the URI string for the container, including the SAS token.
            return string.Format(CultureInfo.InvariantCulture, "{0}{1}", blob.Uri, sasBlobToken);
        }

        public static ServiceProperties ConfigureCors(bool forceConfig=false)
        {
            var storageAccount = GetStorageAccount();
            var blobClient = storageAccount.CreateCloudBlobClient();

            var newProperties = GetCurrentProperties(blobClient);

            //if there aren't any rules present, set them
            //we'll run this on every app start - comment out of global.asax to discontinue
            var addRule = (newProperties.Cors.CorsRules.Count == 0);

            newProperties.DefaultServiceVersion = "2013-08-15";
            blobClient.SetServiceProperties(newProperties);

            if (addRule || forceConfig)
            {
                var ruleWideOpenWriter = new CorsRule()
                {
                    AllowedHeaders = ALLOWED_CORS_HEADERS,
                    AllowedOrigins = ALLOWED_CORS_ORIGINS,
                    AllowedMethods = ALLOWED_CORS_METHODS,
                    ExposedHeaders = new List<string> { "content-length" },
                    MaxAgeInSeconds = (int)TimeSpan.FromDays(ALLOWED_CORS_AGE_DAYS).TotalSeconds
                };
                newProperties.Cors.CorsRules.Clear();
                newProperties.Cors.CorsRules.Add(ruleWideOpenWriter);
                blobClient.SetServiceProperties(newProperties);

                return blobClient.GetServiceProperties();
            }
            return newProperties;
        }

        public static ServiceProperties GetCurrentProperties(CloudBlobClient blobClient)
        {
            return blobClient.GetServiceProperties();
        }

        public static string GetFormattedServiceProperties(ServiceProperties currentProperties, bool detailed=false)
        {
            var res = new StringBuilder();

            if (currentProperties != null)
            {
                if (detailed)
                {
                    if (currentProperties.Cors != null)
                    {
                        res.AppendLine("Cors.CorsRules.Count          : " + currentProperties.Cors.CorsRules.Count);
                        for (int index = 0; index < currentProperties.Cors.CorsRules.Count; index++)
                        {
                            var corsRule = currentProperties.Cors.CorsRules[index];
                            res.AppendLine("corsRule[index]              : " + index);
                            foreach (var allowedHeader in corsRule.AllowedHeaders)
                            {
                                res.AppendLine("corsRule.AllowedHeaders      : " + allowedHeader);
                            }
                            res.AppendLine("corsRule.AllowedMethods      : " + corsRule.AllowedMethods);

                            foreach (var allowedOrigins in corsRule.AllowedOrigins)
                            {
                                res.AppendLine("corsRule.AllowedOrigins      : " + allowedOrigins);
                            }
                            foreach (var exposedHeaders in corsRule.ExposedHeaders)
                            {
                                res.AppendLine("corsRule.ExposedHeaders      : " + exposedHeaders);
                            }
                            res.AppendLine("corsRule.MaxAgeInSeconds     : " + corsRule.MaxAgeInSeconds);
                        }
                    }
                    res.AppendLine("HourMetrics.MetricsLevel      : " + currentProperties.HourMetrics.MetricsLevel);
                    res.AppendLine("HourMetrics.RetentionDays     : " + currentProperties.HourMetrics.RetentionDays);
                    res.AppendLine("HourMetrics.Version           : " + currentProperties.HourMetrics.Version);
                    res.AppendLine("MinuteMetrics.MetricsLevel    : " + currentProperties.MinuteMetrics.MetricsLevel);
                    res.AppendLine("MinuteMetrics.RetentionDays   : " + currentProperties.MinuteMetrics.RetentionDays);
                    res.AppendLine("MinuteMetrics.Version         : " + currentProperties.MinuteMetrics.Version);
                }
                res.AppendLine("DefaultServiceVersion         : " + currentProperties.DefaultServiceVersion);
                res.AppendLine("Logging.LoggingOperations     : " + currentProperties.Logging.LoggingOperations);
                res.AppendLine("Logging.RetentionDays         : " + currentProperties.Logging.RetentionDays);
                res.AppendLine("Logging.Version               : " + currentProperties.Logging.Version);
            }
            //res.Replace(Environment.NewLine, "<br>" + Environment.NewLine);
            return res.ToString();
        }
    }
}