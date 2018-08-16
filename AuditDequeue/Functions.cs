using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Wutnu.Models;
using Wutnu.Common;

namespace AuditDequeue
{
    public class Functions
    {
        /// <summary>
        /// When the method completes, the queue message is deleted. If the method fails before completing, the queue message 
        /// is not deleted; after a 10-minute lease expires, the message is released to be picked up again and processed. This 
        /// sequence won't be repeated indefinitely if a message always causes an exception. After 5 unsuccessful attempts to 
        /// process a message, the message is moved to a queue named {queuename}-poison. The maximum number of attempts is 
        /// configurable.
        /// 
        /// https://azure.microsoft.com/en-us/documentation/articles/websites-dotnet-webjobs-sdk-get-started/#configure-storage
        /// </summary>
        /// <param name="auditInfo"></param>
        /// <param name="logger"></param>
        public static void AddRetrievalAuditLogRecord([QueueTrigger("%queueName%")] AuditDataModel auditInfo, TextWriter logger)
        {
            using (var io = new Wutnu.Data.WutNuContext())
            {
                try
                {
                    io.usp_AddHistory(auditInfo.ShortUrl, auditInfo.CallDate, auditInfo.UserId, auditInfo.HostIp);
                    io.SaveChanges();
                }
                catch (Exception ex)
                {
                    Logging.WriteDebugInfoToErrorLog("Error dequeing", ex, io);
                    var x = ex.GetBaseException();
                    //logger.WriteLine("Error dequeuing: {0}, {1}", x.Message, x.StackTrace);
                    
                    throw ex;
                }
            }
        }
    }
}
