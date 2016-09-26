using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Queue; // Namespace for Queue storage types
using Wutnu.Common;

namespace Wutnu.Repo
{
    /// <summary>
    /// todo: queue to store caller history for dumping into SQL via web job
    /// </summary>
    public static class WutQueue
    {
        public static CloudQueueClient GetQueueClient()
        {
            // Parse the connection string and return a reference to the storage account.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Settings.StorageConnectionString);
            return storageAccount.CreateCloudQueueClient();
        }

        public static CloudQueue GetQueue(CloudQueueClient client, string queueName)
        {
            // Retrieve a reference to a container.
            var queue = client.GetQueueReference(queueName);

            // Create the queue if it doesn't already exist
            queue.CreateIfNotExists();
            return queue;
        }

        public static void AddMessage(CloudQueue queue, string msg)
        {
            // Create a message and add it to the queue.
            CloudQueueMessage message = new CloudQueueMessage(msg);
            queue.AddMessage(message);
        }

        public static string Peek(CloudQueue queue)
        {
            // Peek at the next message
            CloudQueueMessage peekedMessage = queue.PeekMessage();

            return peekedMessage.AsString;
        }

        public static void Update(CloudQueue queue, string msg)
        {
            // Get the message from the queue and update the message contents.
            CloudQueueMessage message = queue.GetMessage();
            message.SetMessageContent(msg);

            queue.UpdateMessage(message,
                TimeSpan.FromSeconds(60.0),  // Make it visible for another 60 seconds.
                MessageUpdateFields.Content | MessageUpdateFields.Visibility);
        }

        public static CloudQueueMessage GetMessage(CloudQueue queue)
        {
            return queue.GetMessage();
        }
        /// <summary>
        /// Asynchronously retrieve the next message in the queue.
        /// </summary>
        /// <param name="queue"></param>
        /// <returns></returns>
        public static async Task<CloudQueueMessage> GetMessageAsync(CloudQueue queue)
        {
            // Async dequeue the message
            return await queue.GetMessageAsync();
            //retrievedMessage.AsString;
        }

        public static void DeleteMessage(CloudQueue queue, CloudQueueMessage msg)
        {
            queue.DeleteMessage(msg);
        }
        public static async void DeleteMessageAsync(CloudQueue queue, CloudQueueMessage msg)
        {
            await queue.DeleteMessageAsync(msg);
        }

        public static async void AddMessageAsync(CloudQueue queue, string msg)
        {
            // Create a message to put in the queue
            CloudQueueMessage cloudQueueMessage = new CloudQueueMessage("My message");

            // Async enqueue the message
            await queue.AddMessageAsync(cloudQueueMessage);
        }

    }
}
