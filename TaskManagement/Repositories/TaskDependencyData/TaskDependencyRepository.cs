using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagement.Repositories.TaskDependencyData
{
    public class TaskDependencyRepository : BaseRepository<TaskDependencyEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserDataRepository"/> class.
        /// </summary>
        /// <param name="configuration">Represents the application configuration.</param>
        /// <param name="isFromAzureFunction">Flag to show if created from Azure Function.</param>
        public TaskDependencyRepository(IConfiguration configuration, bool isFromAzureFunction = false)
            : base(
                configuration,
                PartitionKeyNames.TaskDependencyDataTable.TableName,
                PartitionKeyNames.TaskDependencyDataTable.TaskDependencyDataPartition,
                isFromAzureFunction)
        {
        }

        public async Task<TaskDependencyEntity> GetTaskDependencyDetailsByTaskIDAsync(Guid taskId)
        {
            var allRows = await this.GetAllAsync(PartitionKeyNames.TaskDependencyDataTable.TableName);
            TaskDependencyEntity taskDependencyEntity = allRows.Where(c => c.TaskID == taskId).FirstOrDefault();
            return taskDependencyEntity;
        }
    }
}
