using Microsoft.Azure.WebJobs;
using System.Configuration;

namespace AuditDequeue
{
    public class QueueNameResolver : INameResolver
    {
        private static string queueName;

        public string Resolve(string name)
        {
            if (queueName == null)
            {
                queueName = ConfigurationManager.AppSettings["AuditQueueName"];
            }
            return queueName;
        }
    }
}
