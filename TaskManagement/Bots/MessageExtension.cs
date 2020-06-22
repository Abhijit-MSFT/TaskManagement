using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Bogus;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using TaskManagement.Model;
using TaskManagement.Helper;
using AdaptiveCards;
using TaskManagement.Repositories.TaskDetailsData;
using System.Linq;
using TaskManagement.Repositories.UserDetailsData;
using TaskManagement.Repositories;

namespace TaskManagement
{
    public class MessageExtension : TeamsActivityHandler
    {
        private readonly IConfiguration _configuration;
        private readonly CardHelper _cardHelper;
        private readonly TaskDataRepository _taskDataRepository;
        private readonly UserDetailsRepository _userDetailsRepository;
        private const string PersonalType = "personal";
        private const string ChannelType = "channel";
        private const string groupChatType = "groupChat";

        public MessageExtension(IConfiguration configuration)
        {
            _configuration = configuration;
            _cardHelper = new CardHelper(_configuration);
            _taskDataRepository = new TaskDataRepository(_configuration);
            _userDetailsRepository = new UserDetailsRepository(_configuration);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Text == "Add me")
            {
                var member = await TeamsInfo.GetMemberAsync(turnContext, turnContext.Activity.From.Id, cancellationToken);
                var user = new UserDetailsEntity()
                {
                    Name = (member.Name).Split(" ")[0],
                    UserUniqueID = turnContext.Activity.From.Id,
                    AadId = turnContext.Activity.From.AadObjectId,
                    EmailId = member.Email,
                    ProfilePictureURL = string.Empty,
                    RowKey = Guid.NewGuid().ToString(),
                    PartitionKey = PartitionKeyNames.UserDetailsDataTable.TableName
                };

                await _userDetailsRepository.CreateOrUpdateAsync(user);

                var reply = MessageFactory.Text("Your data is recorded !");
                await turnContext.SendActivityAsync(reply, cancellationToken);
            }
            else
            {
                var reply = MessageFactory.Text("Welcome to Task Manager, Try crating new Tasks using Messaging extension");
                await turnContext.SendActivityAsync(reply, cancellationToken);
            }
        }

        protected override async Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {

            TaskData reldata = JsonConvert.DeserializeObject<TaskData>(taskModuleRequest.Data.ToString());
            var taskTitle = "Edit task";
            if (!string.IsNullOrEmpty(reldata.data.CreateType))
            {
                taskTitle = "Create task";
                if (reldata.data.CreateType == "Depends on")
                {
                    reldata.data.URL += $" ?&createType=Depends on&parentTaskId={reldata.data.TaskId}";
                }
                else
                {
                    reldata.data.URL += $" ?&createType=Blocks&parentTaskId={reldata.data.TaskId}";
                }
            }

            return new TaskModuleResponse
            {
                Task = new TaskModuleContinueResponse
                {
                    Value = new TaskModuleTaskInfo()
                    {
                        Height = 620,
                        Width = 800,
                        Title = taskTitle,
                        Url = reldata.data.URL
                    },
                },
            };
        }
        protected override async Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            TaskInfo taskInfo;
            try
            {
                taskInfo = JsonConvert.DeserializeObject<TaskInfo>(taskModuleRequest.Data.ToString());
            }
            catch (Exception e)
            {
                throw;
            }

            switch (taskInfo.action)
            {
                case "Depends on":
                case "Blocks":
                case "updateAdaptiveCard":
                    try
                    {
                        var name = (turnContext.Activity.From.Name).Split();
                        taskInfo.taskCreatedBy = name[0] + ' ' + name[1];
                        taskInfo.taskCreatedByEmail = await DBHelper.GetUserEmailId(turnContext);
                        taskInfo.allDependentTaskIDs = await _taskDataRepository.GetAllTaskIDsAndTitles(taskInfo.dependentOn);
                        taskInfo.allBlocksTaskIDs = await _taskDataRepository.GetAllTaskIDsAndTitles(taskInfo.blocks);
                        await DBHelper.SaveTaskInfo(taskInfo, _configuration);

                        var typingActivity = MessageFactory.Text(string.Empty);
                        typingActivity.Type = ActivityTypes.Typing;
                        await turnContext.SendActivityAsync(typingActivity);

                        var adaptiveCard = _cardHelper.TaskInformationCard(taskInfo);
                        //below line is to send card to subscribers
                        await Common.SendNotification(turnContext, cancellationToken, _configuration, "v-abjodh@microsoft.com", adaptiveCard);
                        var reply = MessageFactory.Attachment(new Attachment { ContentType = AdaptiveCard.ContentType, Content = adaptiveCard });
                        var result = await turnContext.SendActivityAsync(reply, cancellationToken);
                        //reply.Id = "f:8691943635866570258";
                        //await turnContext.UpdateActivityAsync(reply, cancellationToken);
                        //if (string.IsNullOrEmpty(taskInfo.ParentTaskName))
                        //{ 

                        //}

                        //var newActivity = MessageFactory.Text("The new text for the activity");
                        //message.Id = turnContext.Activity.ReplyToId;
                        //await turnContext.UpdateActivityAsync(message, cancellationToken);

                    }
                    catch (Exception e)
                    {

                        throw;
                    }
                    return null;
                default:
                    return null;
            }
        }


        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionSubmitActionAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            TaskInfo taskInfo;
            try
            {
                taskInfo = JsonConvert.DeserializeObject<TaskInfo>(action.Data.ToString());
            }
            catch (Exception e)
            {
                throw;
            }

            switch (taskInfo.action)
            {
                case "sendAdaptiveCard":
                    try
                    {
                        var name = (turnContext.Activity.From.Name).Split();
                        taskInfo.taskCreatedBy = name[0] + ' ' + name[1];
                        taskInfo.taskCreatedByEmail = await DBHelper.GetUserEmailId(turnContext);
                        taskInfo.allDependentTaskIDs = await _taskDataRepository.GetAllTaskIDsAndTitles(taskInfo.dependentOn);
                        taskInfo.allBlocksTaskIDs = await _taskDataRepository.GetAllTaskIDsAndTitles(taskInfo.blocks);
                        CardHelper cardhelper = new CardHelper(_configuration);
                        var attPath = taskInfo.attachements;
                        await DBHelper.SaveTaskInfo(taskInfo, _configuration);
                        var typingActivity = MessageFactory.Text(string.Empty);
                        typingActivity.Type = ActivityTypes.Typing;
                        await turnContext.SendActivityAsync(typingActivity);
                        var adaptiveCard = cardhelper.TaskInformationCard(taskInfo);
                        var reply = MessageFactory.Attachment(new Attachment { ContentType = AdaptiveCard.ContentType, Content = adaptiveCard });
                        await turnContext.SendActivityAsync(reply, cancellationToken);

                    }
                    catch (Exception e)
                    {

                        throw;
                    }
                    return null;
                default:
                    return null;
            }
        }

        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionFetchTaskAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            var TaskDesFromPayload = action.MessagePayload.Body.Content;

            var response = new MessagingExtensionActionResponse()
            {
                Task = new TaskModuleContinueResponse()
                {
                    Value = new TaskModuleTaskInfo()
                    {
                        Height = 620,
                        Width = 800,
                        //Title = "Invite people to share how they feel",
                        Url = this._configuration["BaseUri"] + "/CreateNewTask/" + TaskDesFromPayload
                    },
                },
            };

            return response;
        }

        protected override async Task<MessagingExtensionResponse> OnTeamsMessagingExtensionQueryAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query, CancellationToken cancellationToken)
        {
            var queryText = string.Empty;
            var taskParam = query.Parameters?.FirstOrDefault(p => p.Name == "taskTitle");
            if (taskParam != null)
            {
                queryText = taskParam.Value.ToString();
            }
            string useremail = "Gousia Begum";
            List<TaskDataEntity> userTasksData = new List<TaskDataEntity>();
            switch (query.CommandId)
            {
                case "MyTasks":
                    userTasksData = await _taskDataRepository.GetUserTasksAsync(useremail);
                    break;
                case "SubscribedTasks":
                    userTasksData = await _taskDataRepository.GetUserSubscribedTasksAsync(useremail);
                    break;
                default:
                    break;
            }

            var filteredTask = userTasksData;
            if (!string.IsNullOrEmpty(queryText))
            {
                filteredTask = userTasksData.Where(task => task.TaskName.ToLower() == queryText.ToLower() || task.TaskTitle.ToLower() == queryText.ToLower()).ToList();
            }

            if (filteredTask.Count == 0)
            {
                return new MessagingExtensionResponse
                {
                    ComposeExtension = new MessagingExtensionResult
                    {
                        Type = "message",
                        Text = "No match found.",
                    },
                };
            }

            var taskattachments = new List<MessagingExtensionAttachment>();
            foreach (var task in filteredTask)
            {
                var previewCard = new ThumbnailCard
                {
                    Title = task.TaskName + " - " + task.TaskTitle,
                    Text = $"Owner - {task.TaskAssignedTo}" +
                  $" ({ task.TaskStatus})",
                    Images = new List<CardImage>() { new CardImage { Url = _configuration["BaseUri"] + "/Images/" + task.TaskPriority + ".png" } }
                };
                var adaptiveCard = _cardHelper.TaskInformationCard(await CreateTaskInfoFromTaskEntity(task));
                taskattachments.Add(new MessagingExtensionAttachment
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = adaptiveCard,
                    Preview = previewCard.ToAttachment(),
                });

            }


            // The list of MessagingExtensionAttachments must we wrapped in a MessagingExtensionResult wrapped in a MessagingExtensionResponse.
            return new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "result",
                    AttachmentLayout = "list",
                    Attachments = taskattachments
                }
            };
        }

        private async Task<TaskInfo> CreateTaskInfoFromTaskEntity(TaskDataEntity task)
        {
            return new TaskInfo()
            {
                title = task.TaskTitle,
                priority = task.TaskPriority,
                taskAssignedTo = task.TaskAssignedTo,
                status = task.TaskStatus,
                taskID = task.TaskId,
                taskNumber = task.TaskName,
                blocks = task.Blocks.ToList(),
                allDependentTaskIDs = await _taskDataRepository.GetAllTaskIDsAndTitles(task.Dependencies.ToList()),
                allBlocksTaskIDs = await _taskDataRepository.GetAllTaskIDsAndTitles(task.Blocks.ToList()),


            };
        }

        private static MessagingExtensionAttachment GetAttachment(string title)
        {
            var card = new ThumbnailCard
            {
                Title = !string.IsNullOrWhiteSpace(title) ? title : new Faker().Lorem.Sentence(),
                Text = new Faker().Lorem.Paragraph(),
                Images = new List<CardImage> { new CardImage("http://lorempixel.com/640/480?rand=" + DateTime.Now.Ticks.ToString()) }
            };

            return card
                .ToAttachment()
                .ToMessagingExtensionAttachment();
        }

        protected override Task<MessagingExtensionResponse> OnTeamsMessagingExtensionSelectItemAsync(ITurnContext<IInvokeActivity> turnContext, JObject query, CancellationToken cancellationToken)
        {

            return Task.FromResult(new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    AttachmentLayout = "list",
                    Type = "result",
                    Attachments = new MessagingExtensionAttachment[]{
                        new ThumbnailCard()
                            .ToAttachment()
                            .ToMessagingExtensionAttachment()
                    }
                },
            });
        }

        protected async override Task OnTeamsMessagingExtensionCardButtonClickedAsync(ITurnContext<IInvokeActivity> turnContext, JObject cardData, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text("OnTeamsMessagingExtensionCardButtonClickedAsync Value: " + JsonConvert.SerializeObject(turnContext.Activity.Value));
            await turnContext.SendActivityAsync(reply, cancellationToken);

            //return base.OnTeamsMessagingExtensionCardButtonClickedAsync(turnContext, cardData, cancellationToken);
        }

        /// <summary>
        /// Send user message to the datebase
        /// </summary>
        /// <param name="membersAdded">membersAdded</param>
        /// <param name="turnContext">turnContext</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
        }

        /// <summary>
        /// Invoked when a conversation update activity is received from the channel.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnConversationUpdateActivityAsync(
            ITurnContext<IConversationUpdateActivity> turnContext,
            CancellationToken cancellationToken)
        {
            // base.OnConversationUpdateActivityAsync is useful when it comes to responding to users being added to or removed from the conversation.
            // For example, a bot could respond to a user being added by greeting the user.
            // By default, base.OnConversationUpdateActivityAsync will call <see cref="OnMembersAddedAsync(IList{ChannelAccount}, ITurnContext{IConversationUpdateActivity}, CancellationToken)"/>
            // if any users have been added or <see cref="OnMembersRemovedAsync(IList{ChannelAccount}, ITurnContext{IConversationUpdateActivity}, CancellationToken)"/>
            // if any users have been removed. base.OnConversationUpdateActivityAsync checks the member ID so that it only responds to updates regarding members other than the bot itself.
            await base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);

            var activity = turnContext.Activity;
            var botId = activity.Recipient.Id;

            // Take action if this event includes the bot being added
            if (activity.MembersAdded?.FirstOrDefault(p => p.Id == botId) != null)
            {
                await this.OnBotAddedAsync(turnContext);
            }

            // Take action if this event includes the bot being removed
            if (activity.MembersRemoved?.FirstOrDefault(p => p.Id == botId) != null)
            {
                await this.OnBotRemovedAsync(turnContext);
            }
        }

        public async Task OnBotAddedAsync(ITurnContext<IConversationUpdateActivity> turnContext)
        {
            switch (turnContext.Activity.Conversation.ConversationType)
            {
                case ChannelType:
                case groupChatType:

                    var members = await turnContext.GetConversationMembers();
                    var teamMembers = TurnContextExtension.AsTeamsChannelAccounts(members).ToList();

                    await _userDetailsRepository.SaveAllUserDetailsInTeams(turnContext, teamMembers);
                    break;
                case PersonalType:
                    await this._userDetailsRepository.SaveUserDetailsAsync(turnContext);
                    break;
                default: break;
            }
        }

        public async Task OnBotRemovedAsync(ITurnContext<IConversationUpdateActivity> turnContext)
        {
        }

    }

}
