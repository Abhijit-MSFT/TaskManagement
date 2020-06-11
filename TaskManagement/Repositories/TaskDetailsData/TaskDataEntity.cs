using System;
using System.Collections.Generic;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;

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
        

        public string SubscribersString { get; set; }

        /// <summary>
        /// Gets or sets Rosters audience collection.
        /// </summary>
        [IgnoreProperty]
        public IEnumerable<string> Subscribers
        {
            get
            {
                return JsonConvert.DeserializeObject<IEnumerable<string>>(this.SubscribersString);
            }

            set
            {
                this.SubscribersString = JsonConvert.SerializeObject(value);
            }
        }

        public string DependencyString { get; set; }

        /// <summary>
        /// Gets or sets Rosters audience collection.
        /// </summary>
        [IgnoreProperty]
        public IEnumerable<string> Dependencies
        {
            get
            {
                return JsonConvert.DeserializeObject<IEnumerable<string>>(this.DependencyString);
            }

            set
            {
                this.DependencyString = JsonConvert.SerializeObject(value);
            }
        }

        public string BlocksString { get; set; }

        /// <summary>
        /// Gets or sets Rosters audience collection.
        /// </summary>
        [IgnoreProperty]
        public IEnumerable<string> Blocks
        {
            get
            {
                return JsonConvert.DeserializeObject<IEnumerable<string>>(this.BlocksString);
            }

            set
            {
                this.BlocksString = JsonConvert.SerializeObject(value);
            }
        }

    }
}
