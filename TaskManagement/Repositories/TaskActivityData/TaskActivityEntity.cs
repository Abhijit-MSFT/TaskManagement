using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagement.Repositories.TaskActivityData
{
    public class TaskActivityEntity : TableEntity
    {
        public Guid TaskActivityID { get; set; }
        public Guid TaskID { get; set; }
        public string ActivityCreatedBy { get; set; }
        public string ActivityCreatedByEmail { get; set; }
        public string ActivityCreatedDate { get; set; }
        public string ActivityCreatedTime { get; set; }
        public string Comments { get; set; }
    }
}


