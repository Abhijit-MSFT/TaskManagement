using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagement.Repositories.TaskAttachementsData
{
    public class TaskAttachementsRepository : BaseRepository<TaskAttachementsEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserDataRepository"/> class.
        /// </summary>
        /// <param name="configuration">Represents the application configuration.</param>
        /// <param name="isFromAzureFunction">Flag to show if created from Azure Function.</param>
        public TaskAttachementsRepository(IConfiguration configuration, bool isFromAzureFunction = false)
            : base(
                configuration,
                PartitionKeyNames.TaskAttachementsDataTable.TableName,
                PartitionKeyNames.TaskAttachementsDataTable.TaskAttachementsDataPartition,
                isFromAzureFunction)
        {
        }
    }
}
