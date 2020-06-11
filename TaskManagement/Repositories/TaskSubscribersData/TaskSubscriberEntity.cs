using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagement.Repositories.TaskSubscribersData
{
    public class TaskSubscriberEntity : TableEntity
    {
        public Guid TaskSubscriberID { get; set; }
        public Guid TaskID { get; set; }
        public List<string>? SubscriberName { get; set; }
        public string? SubscriberEmail { get; set; }
    }
}
