using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagement.Repositories.TaskAttachementsData
{
    public class TaskAttachementsEntity : TableEntity
    {
        public Guid AttachementID { get; set; }
        public Guid TaskID { get; set; }
        public string? AttachementURL { get; set; }
    }
}
