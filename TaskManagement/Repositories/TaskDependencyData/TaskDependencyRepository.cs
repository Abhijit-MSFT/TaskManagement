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
    }
}
