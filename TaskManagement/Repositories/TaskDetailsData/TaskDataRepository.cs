using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagement.Repositories.TaskDetailsData
{
    public class TaskDataRepository : BaseRepository<TaskDataEntity>
    {
        private Dictionary<string, object> lastfetchedDataDict = new Dictionary<string, object>(); // cache Dictionary
        private readonly string cacheDurationInMinutes;
        private DateTime lastFetchTimeStamp;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserDataRepository"/> class.
        /// </summary>
        /// <param name="configuration">Represents the application configuration.</param>
        /// <param name="isFromAzureFunction">Flag to show if created from Azure Function.</param>
        public TaskDataRepository(IConfiguration configuration, bool isFromAzureFunction = false)
            : base(
                configuration,
                PartitionKeyNames.TaskDetailsDataTable.TableName,
                PartitionKeyNames.TaskDetailsDataTable.TaskDataPartition,
                isFromAzureFunction)
        {
            this.cacheDurationInMinutes = configuration["CacheDurationInMinutes"];
        }

        public async Task<string> GetLastCreatedTaskID()
        {
            var allRows = await this.GetAllAsync(PartitionKeyNames.TaskDetailsDataTable.TableName);
            string LastCreatedTaskID = allRows.OrderByDescending(c => c.TaskName).FirstOrDefault().ToString();
            return LastCreatedTaskID;
        }

        public async Task<TaskDataEntity> GetTaskDetailsByTaskIDAsync(Guid taskId)
        {
            var allRows = await this.GetAllAsync(PartitionKeyNames.TaskDetailsDataTable.TableName);
            TaskDataEntity taskDataEntity = allRows.Where(c => c.TaskId == taskId).FirstOrDefault();
            return taskDataEntity;
        }

        public async Task<Dictionary<string, string>> GetAllTaskIDAsync()
        {
            var allRows = await this.GetAllAsync(PartitionKeyNames.TaskDetailsDataTable.TableName);
            Dictionary<string, string> allIdsandTitles = allRows.ToDictionary(x => x.TaskName, y => y.TaskTitle);
            return allIdsandTitles;
        }

        public async Task<List<TaskDataEntity>> GetUserTasksAsync(string email)
        {
            var allRows = await this.GetAllAsync(PartitionKeyNames.TaskDetailsDataTable.TableName);
            List<TaskDataEntity> userTasks = allRows.Where(c => c.TaskAssignedTo == email).ToList();
            return userTasks;
        }

        //change below query to get subscribed tasks
        public async Task<List<TaskDataEntity>> GetUserSubscribedTasksAsync(string email)
        {
            var allRows = await this.GetAllAsync(PartitionKeyNames.TaskDetailsDataTable.TableName);
            List<TaskDataEntity> userTasks = allRows.Where(c => c.TaskAssignedTo == email).ToList();
            return userTasks;
        }


        public async Task<List<string>> GetAllTaskIDsAndTitles(List<string> ids)
        {
            if (ids == null || ids.Count == 0)
                return null;

            var dictKey = "GetAllTaskIDsAndTitles";
            IEnumerable<TaskDataEntity> allRows;
            if (IsCachedDataExpired(dictKey))
            {
                allRows = await this.GetAllAsync(PartitionKeyNames.TaskDetailsDataTable.TableName);
                this.UpdateCache(dictKey, allRows);
            }
            else
            {
                allRows = (IEnumerable<TaskDataEntity>)this.lastfetchedDataDict[dictKey];
            }
            List<TaskDataEntity> filteredRows = allRows.Where(c => ids.Contains(c.TaskName)).ToList();
            List<string> IdsAndTitles = filteredRows.Select(x => x.TaskName + " - " + x.TaskTitle).ToList();
            return IdsAndTitles;
        }

        private bool IsCachedDataExpired(string cacheKey)
        {
            if (!this.lastfetchedDataDict.ContainsKey(cacheKey))
            {
                return true;
            }
            else
            {
                var currentTime = DateTime.Now;
                int.TryParse(this.cacheDurationInMinutes, out int cacheDuration);
                if (currentTime.AddMinutes(-cacheDuration) > this.lastFetchTimeStamp)
                {
                    return true;
                }
            }

            return false;
        }

        private void UpdateCache<T>(string cacheKey, T result)
        {
            // Make sure that we are storing not null values.
            if (result == null)
            {
                return;
            }

            this.lastFetchTimeStamp = DateTime.Now;

            if (!this.lastfetchedDataDict.ContainsKey(cacheKey))
            {
                this.lastfetchedDataDict.Add(cacheKey, result);
            }
            else
            {
                this.lastfetchedDataDict[cacheKey] = result;
            }
        }
    }
}
