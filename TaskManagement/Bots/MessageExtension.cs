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

namespace TaskManagement
{
    //TaskManagement
    public class MessageExtension : TeamsActivityHandler
    {
        private readonly IConfiguration _configuration;

        public MessageExtension(IConfiguration configuration)
        {
            _configuration = configuration;

        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {

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
                        TaskDataRepository taskDataRepository = new TaskDataRepository(_configuration);
                        taskInfo.akkTaskIDs = await taskDataRepository.GetAllTaskIDsAndTitles(taskInfo.dependentOn);
                        await DBHelper.SaveTaskInfo(taskInfo, _configuration);

                        CardHelper cardhelper = new CardHelper(_configuration);
                        var typingActivity = MessageFactory.Text(string.Empty);
                        typingActivity.Type = ActivityTypes.Typing;
                        await turnContext.SendActivityAsync(typingActivity);
                        var adaptiveCard = cardhelper.TaskInformationCard(taskInfo);
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
                        TaskDataRepository taskDataRepository = new TaskDataRepository(_configuration);
                        taskInfo.akkTaskIDs = await taskDataRepository.GetAllTaskIDsAndTitles(taskInfo.dependentOn);
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
            var text = query?.Parameters?[0]?.Value as string ?? string.Empty;
            string useremail = "Gousia Begum";
            TaskDataRepository taskDataRepository = new TaskDataRepository(_configuration);
            List<TaskDataEntity> userTasksData = new List<TaskDataEntity>();
            switch (query.CommandId)
            {                
                case "MyTasks":            
                    userTasksData = await taskDataRepository.GetUserTasksAsync(useremail);
                    break;
                case "SubscribedTasks":
                    userTasksData = await taskDataRepository.GetUserSubscribedTasksAsync(useremail);
                    break;
                default:
                    break;
            }

            var attachments = userTasksData.Select(c => new MessagingExtensionAttachment
            {
                ContentType = HeroCard.ContentType,
                Content = new HeroCard { Title = c.TaskTitle },
                Preview = new HeroCard
                {
                    Title = c.TaskName + " - " + c.TaskTitle,
                    Subtitle = "Owner - " + c.TaskAssignedTo,
                    Text = "Status - " + c.TaskStatus,
                    Tap = new CardAction { Type = "invoke", Value = c }
                }.ToAttachment()
            }).ToList();



            // The list of MessagingExtensionAttachments must we wrapped in a MessagingExtensionResult wrapped in a MessagingExtensionResponse.
            return new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "result",
                    AttachmentLayout = "list",
                    Attachments = attachments
                }
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
    }

}
