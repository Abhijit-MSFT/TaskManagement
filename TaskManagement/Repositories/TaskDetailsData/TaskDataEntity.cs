using System;
using Microsoft.Azure.Cosmos.Table;


namespace TaskManagement.Repositories.TaskDetailsData
{
    public class TaskDataEntity : TableEntity
    {
        public string TaskName { get; set; }
        public Guid TaskId { get; set; }
        public string TaskCreatedBy { get; set; }
        public string TaskAssignedTo { get; set; }
        public string TaskCreatedByEmail { get; set; }
        public string TaskStatus { get; set; }
        public string TaskPriority { get; set; }
        public string TaskTitle { get; set; }
        public string TaskDescription { get; set; }
        public DateTime TaskStartDate { get; set; }
        public DateTime TaskDueDate { get; set; }
        public Guid AttachementID { get; set; }
        public Guid SubscriberID { get; set; }
        public Guid DependencyID { get; set; }
        public string Blocks { get; set; }
        public Guid TaskActivityID { get; set; }
    }
}
