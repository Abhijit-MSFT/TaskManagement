using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagement.Repositories.TaskSubscribersData
{
    public class TaskSubscriberRepository : BaseRepository<TaskSubscriberEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserDataRepository"/> class.
        /// </summary>
        /// <param name="configuration">Represents the application configuration.</param>
        /// <param name="isFromAzureFunction">Flag to show if created from Azure Function.</param>
        public TaskSubscriberRepository(IConfiguration configuration, bool isFromAzureFunction = false)
            : base(
                configuration,
                PartitionKeyNames.TaskSubscribersDataTable.TableName,
                PartitionKeyNames.TaskSubscribersDataTable.TaskSubscribersDataPartition,
                isFromAzureFunction)
        {
        }

        public async Task<TaskSubscriberEntity> GetTaskSubscribersDetailsByTaskIDAsync(Guid taskId)
        {
            var allRows = await this.GetAllAsync(PartitionKeyNames.TaskSubscribersDataTable.TableName);
            TaskSubscriberEntity taskSubscriberEntity = allRows.Where(c => c.TaskID == taskId).FirstOrDefault();
            return taskSubscriberEntity;
        }
    }
}
