using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using TaskManagement.Repositories.TaskDetailsData;
using TaskManagement.Repositories.UserDetailsData;

namespace TaskManagement.Helper
{
    public class Common
    {
        private readonly IConfiguration _configuration;

        public Common(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetNewTaskID()
        {
            //TaskDataRepository taskDataRepository = new TaskDataRepository(_configuration);
            //var lastCreatedID = taskDataRepository.GetLastCreatedTaskID();

            Random r = new Random();
            string NewID = "T" + r.Next(1000, 2000);
            return NewID;
            //using (var rng = new RNGCryptoServiceProvider())
            //{
            //    var bit_count = (inputString * 6);
            //    var byte_count = ((bit_count + 7) / 8); // rounded up
            //    var bytes = new byte[byte_count];
            //    rng.GetBytes(bytes);
            //    return Convert.ToBase64String(bytes);
            //}
        }


        public static async Task SendNotification<T>(ITurnContext<T> turnContext, CancellationToken cancellationToken, IConfiguration configuration, string userEmail, AdaptiveCard card) where T : Microsoft.Bot.Schema.IActivity
        {
            var id = configuration["MicrosoftAppId"];
            var pass = configuration["MicrosoftAppPassword"];
            var teamsChannelId = configuration["ChannelID"];
            var serviceUrl = turnContext.Activity.ServiceUrl;
            var credentials = new MicrosoftAppCredentials(id, pass);
            ConversationReference conversationReference = null;

            var user = await GetUserId(userEmail, configuration);
            var proactiveMessage = MessageFactory.Attachment(new Attachment { ContentType = AdaptiveCard.ContentType, Content = card });

            var conversationParameters = new ConversationParameters
            {
                IsGroup = false,
                Bot = turnContext.Activity.Recipient,
                Members = new ChannelAccount[] {
                        new ChannelAccount()
                        {
                            AadObjectId = user.AadId,
                            Id = user.UserUniqueID,
                            Name = user.Name
                        }
                    },
                TenantId = turnContext.Activity.Conversation.TenantId,
            };


            await ((BotFrameworkAdapter)turnContext.Adapter).CreateConversationAsync(teamsChannelId, user.ServiceUrl, credentials, conversationParameters,
                async (t1, c1) =>
                {
                    conversationReference = t1.Activity.GetConversationReference();
                    await ((BotFrameworkAdapter)turnContext.Adapter).ContinueConversationAsync(
                        id,
                        conversationReference,
                        async (t2, c2) =>
                        {
                            try
                            {
                                await t2.SendActivityAsync(proactiveMessage, c2);
                            }
                            catch (Exception e)
                            {

                                Console.WriteLine(e);
                            }
                        },
                        cancellationToken);
                },
                        cancellationToken);
        }

        public static async Task<UserDetailsEntity> GetUserId(string userEmail, IConfiguration configuration)
        {
            var managerId = userEmail;
            UserDetailsRepository userDetailsDataRepository = new UserDetailsRepository(configuration);

            UserDetailsEntity manager = await userDetailsDataRepository.GeUserDetails(managerId.ToLower());

            return manager ?? null;
        }
    }

    public static class TurnContextExtension
    {
        public static async Task<IList<ChannelAccount>> GetConversationMembers(this ITurnContext<IConversationUpdateActivity> turnContext)
        {
            IConnectorClient connector = turnContext.TurnState.Get<IConnectorClient>();
            var channelData = turnContext.Activity.GetChannelData<TeamsChannelData>();
            var conversationId = turnContext.Activity.Conversation.Id;
            if (turnContext.Activity.Conversation.ConversationType == "channel")
            {
                conversationId = channelData.Team.Id;
            }

            var members = await connector.Conversations.GetConversationMembersAsync(conversationId);
            return members;
        }

        public static IEnumerable<TeamsChannelAccount> AsTeamsChannelAccounts(IEnumerable<ChannelAccount> channelAccountList)
        {
            foreach (ChannelAccount channelAccount in channelAccountList)
            {
                yield return JObject.FromObject(channelAccount).ToObject<TeamsChannelAccount>();
            }
        }

    }
}
