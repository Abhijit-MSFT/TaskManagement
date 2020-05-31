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
            return null;
        }

        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionSubmitActionAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            TaskInfo taskInfo = new TaskInfo();
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
                case "sendAdaptiveCard" :
                    try
                    {
                        var name = (turnContext.Activity.From.Name).Split();
                        taskInfo.taskCreatedBy = name[0] + ' ' + name[1];
                        taskInfo.taskCreatedByEmail = await DBHelper.GetUserEmailId(turnContext);
                        await DBHelper.SaveTaskInfo(taskInfo, _configuration);

                        CardHelper cardhelper = new CardHelper(_configuration);
                        //var typingActivity = MessageFactory.Text(string.Empty);
                        //typingActivity.Type = ActivityTypes.Typing;
                        //await turnContext.SendActivityAsync(typingActivity);
                        var adaptiveCard = cardhelper.TaskInformationCard();
                        var message = MessageFactory.Attachment(new Attachment { ContentType = AdaptiveCard.ContentType, Content = adaptiveCard });
                        await turnContext.SendActivityAsync(message, cancellationToken);
                    }
                    catch (Exception e)
                    {

                        throw;
                    }
                   


                    return null;
                default:
                    return null;
            }
            

            return null;
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
                        Url = this._configuration["BaseUri"] + "/CreateNewTask"
        },
                },
            };

            return response;
        }

        protected override Task<MessagingExtensionResponse> OnTeamsMessagingExtensionQueryAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query, CancellationToken cancellationToken)
        {
            
            return null;

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
