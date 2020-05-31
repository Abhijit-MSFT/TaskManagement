using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagement.Repositories.TaskDependencyData
{
    public class TaskDependencyEntity : TableEntity
    {
        public Guid TaskDependencyID { get; set; }
        public Guid TaskID { get; set; }
        public string DependentTaskID { get; set; } //change it to guid - currently string is sent from the frontend
        public string DependentTaskOwner { get; set; }        
    }
}
