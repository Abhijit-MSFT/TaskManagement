using System;
using TaskManagement.Repositories.TaskDetailsData;
using TaskManagement.Repositories.TaskAttachementsData;
using TaskManagement.Repositories.TaskSubscribersData;
using TaskManagement.Repositories.TaskDependencyData;
using TaskManagement.Repositories.TaskActivityData;
using TaskManagement.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManagement.Repositories;
using System.Runtime.CompilerServices;

namespace TaskManagement.Helper
{
    public static class DBHelper
    {
        public static async Task SaveTaskInfo(TaskInfo taskInfo, IConfiguration configuration)
        {
            TaskDataRepository taskDataRepository = new TaskDataRepository(configuration);
            if (taskInfo != null)
            {
                var rowKey = Guid.NewGuid();

                if (taskInfo.action == "sendAdaptiveCard" || taskInfo.action == "Depends on" || taskInfo.action == "Blocks")
                {
                    taskInfo.taskID = Guid.NewGuid();
                    taskInfo.attachementID = Guid.NewGuid();
                    taskInfo.subscriberID = Guid.NewGuid();

                    TaskDataEntity taskDataEntity = new TaskDataEntity
                    {
                        PartitionKey = PartitionKeyNames.TaskDetailsDataTable.TaskDataPartition,
                        RowKey = rowKey.ToString(),
                        TaskName = taskInfo.taskNumber, // change it to auto crated taskName
                        TaskId = taskInfo.taskID,
                        TaskCreatedBy = taskInfo.taskCreatedBy,
                        TaskAssignedTo = taskInfo.taskAssignedTo,
                        TaskCreatedByEmail = taskInfo.taskCreatedByEmail,
                        TaskStatus = taskInfo.status,
                        TaskPriority = taskInfo.priority,
                        TaskTitle = taskInfo.title,
                        TaskDescription = taskInfo.description,
                        TaskStartDate = taskInfo.startDate,
                        TaskDueDate = taskInfo.dueDate,
                        AttachementID = taskInfo.attachementID,
                        Subscribers = taskInfo.subscribers,
                        Dependencies = taskInfo.dependentOn,
                        Blocks = taskInfo.blocks
                    };

                    await taskDataRepository.CreateOrUpdateAsync(taskDataEntity);
                }
                else if (taskInfo.action == "updateAdaptiveCard")
                {
                    TaskDataEntity taskData = await taskDataRepository.GetTaskDetailsByTaskIDAsync(taskInfo.taskID);

                    taskData.TaskAssignedTo = taskInfo.taskAssignedTo;
                    taskData.TaskStatus = taskInfo.status;
                    taskData.TaskPriority = taskInfo.priority;
                    taskData.TaskTitle = taskInfo.title;
                    taskData.TaskDescription = taskInfo.description;
                    taskData.TaskDueDate = taskInfo.dueDate;
                    taskData.Subscribers = taskInfo.subscribers;
                    taskData.Dependencies = taskInfo.dependentOn;
                    taskData.Blocks = taskInfo.blocks;

                    await taskDataRepository.CreateOrUpdateAsync(taskData);

                }

                //check required conditions before pushing data to below tables
                await DBHelper.SaveTaskAttachements(taskInfo, configuration);
                //await DBHelper.SaveSubscribersInfo(taskInfo, configuration);
                //await DBHelper.SaveDependency(taskInfo, configuration);
                await DBHelper.SaveActivity(taskInfo, configuration);

            };

        }


        public static async Task SaveTaskAttachements(TaskInfo taskInfo, IConfiguration configuration)
        {
            TaskAttachementsRepository taskAttachementsRepository = new TaskAttachementsRepository(configuration);
            var rowKey = Guid.NewGuid();

            TaskAttachementsEntity taskAttachementsEntity = new TaskAttachementsEntity
            {
                PartitionKey = PartitionKeyNames.TaskAttachementsDataTable.TaskAttachementsDataPartition,
                RowKey = rowKey.ToString(),
                AttachementID = taskInfo.attachementID,
                TaskID = taskInfo.taskID,
                AttachementURL = taskInfo.attachementURL
            };

            await taskAttachementsRepository.CreateOrUpdateAsync(taskAttachementsEntity);

        }

        public static async Task SaveSubscribersInfo(TaskInfo taskInfo, IConfiguration configuration)
        {
            TaskSubscriberRepository taskSubscriberRepository = new TaskSubscriberRepository(configuration);
            var rowKey = Guid.NewGuid();

            TaskSubscriberEntity taskSubscriberEntity = new TaskSubscriberEntity
            {
                PartitionKey = PartitionKeyNames.TaskSubscribersDataTable.TaskSubscribersDataPartition,
                RowKey = rowKey.ToString(),
                TaskSubscriberID = taskInfo.subscriberID,
                TaskID = taskInfo.taskID,
                SubscriberName = taskInfo.subscribers,
                SubscriberEmail = taskInfo.taskCreatedByEmail //need to change this to subscribers email id 
            };

            await taskSubscriberRepository.CreateOrUpdateAsync(taskSubscriberEntity);
        }

        //public static async Task SaveDependency(TaskInfo taskInfo, IConfiguration configuration)
        //{
        //    TaskDependencyRepository taskDependencyRepository = new TaskDependencyRepository(configuration);
        //    var rowKey = Guid.NewGuid();

        //    TaskDependencyEntity taskDependencyEntity = new TaskDependencyEntity
        //    {
        //        PartitionKey = PartitionKeyNames.TaskDependencyDataTable.TaskDependencyDataPartition,
        //        RowKey = rowKey.ToString(),
        //        TaskDependencyID = taskInfo.dependencyID,
        //        TaskID = taskInfo.taskID,
        //        DependentTaskID = taskInfo.dependentOn
        //    };

        //    await taskDependencyRepository.CreateOrUpdateAsync(taskDependencyEntity);
        //}

        public static async Task SaveActivity(TaskInfo taskInfo, IConfiguration configuration)
        {
            TaskActivityRepository taskActivityRepository = new TaskActivityRepository(configuration);
            var rowKey = Guid.NewGuid();

            TaskActivityEntity taskActivityEntity = new TaskActivityEntity
            {
                PartitionKey = PartitionKeyNames.TaskActivityDataTable.TaskActivityDataPartition,
                RowKey = rowKey.ToString(),
                TaskID = taskInfo.taskID,
                ActivityCreatedBy = taskInfo.taskCreatedBy,
                ActivityCreatedByEmail = taskInfo.taskCreatedByEmail,
                ActivityCreatedDate = DateTime.Now.Date.ToString(),
                ActivityCreatedTime = DateTime.Now.ToString("H:m:ss tt"),
                Comments = taskInfo.activityComment
            };

            await taskActivityRepository.CreateOrUpdateAsync(taskActivityEntity);
        }

        public static async Task<string> GetUserEmailId<T>(ITurnContext<T> turnContext) where T : Microsoft.Bot.Schema.IActivity
        {
            // Fetch the members in the current conversation
            try
            {
                IConnectorClient connector = turnContext.TurnState.Get<IConnectorClient>();
                var members = await connector.Conversations.GetConversationMembersAsync(turnContext.Activity.Conversation.Id);
                return AsTeamsChannelAccounts(members).FirstOrDefault(m => m.Id == turnContext.Activity.From.Id).UserPrincipalName;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public static async Task<List<string>> GetTeamMembers(ITurnContext turnContext)
        {
            try
            {
                IConnectorClient connector = turnContext.TurnState.Get<IConnectorClient>();
                var members = await connector.Conversations.GetConversationMembersAsync(turnContext.Activity.Conversation.Id);
                var mamberNames = members.Select(c => c.Name).ToList();
                return mamberNames;
            }
            catch (Exception ex)
            {
                return null;
            }
        }



        private static IEnumerable<TeamsChannelAccount> AsTeamsChannelAccounts(IEnumerable<ChannelAccount> channelAccountList)
        {
            foreach (ChannelAccount channelAccount in channelAccountList)
            {
                yield return JObject.FromObject(channelAccount).ToObject<TeamsChannelAccount>();
            }
        }

        public static async Task<PageLoadData> GetPageLoadDataAsync(IConfiguration configuration)
        {
            Common common = new Common(configuration);
            TaskDataRepository taskDataRepository = new TaskDataRepository(configuration);
            PageLoadData pageLoadData = new PageLoadData
            {
                //NewTaskId = common.GetNewTaskID(),
                ListofTaskIDs = await taskDataRepository.GetAllTaskIDAsync(),
                //TeamMembers = await DBHelper.GetTeamMembers(turnContext)
            };
            return pageLoadData;
        }
    }
}


