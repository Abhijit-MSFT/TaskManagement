using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagement.Repositories.TaskActivityData
{
    public class TaskActivityRepository : BaseRepository<TaskActivityEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserDataRepository"/> class.
        /// </summary>
        /// <param name="configuration">Represents the application configuration.</param>
        /// <param name="isFromAzureFunction">Flag to show if created from Azure Function.</param>
        public TaskActivityRepository(IConfiguration configuration, bool isFromAzureFunction = false)
            : base(
                configuration,
                PartitionKeyNames.TaskActivityDataTable.TableName,
                PartitionKeyNames.TaskActivityDataTable.TaskActivityDataPartition,
                isFromAzureFunction)
        {
        }

        public async Task<List<TaskActivityEntity>> GetTaskActivityDetailsByTaskIDAsync(Guid taskId)
        {
            var allRows = await this.GetAllAsync(PartitionKeyNames.TaskActivityDataTable.TableName);
            List<TaskActivityEntity> taskActivityEntity = allRows.Where(c => c.TaskID == taskId).ToList();
            return taskActivityEntity;
        }
    }
}
