using AdaptiveCards;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TaskManagement.Model;
using TaskManagement.Repositories.UserDetailsData;

namespace TaskManagement.Helper
{
    public class Common
    {
        private readonly IConfiguration _configuration;
        private static UserDetailsRepository userDataRepository;
        private static string botAccessToken = null;

        public Common(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetNewTaskID()
        {
            //TaskDataRepository taskDataRepository = new TaskDataRepository(_configuration);
            //var lastCreatedID = taskDataRepository.GetLastCreatedTaskID();

            Random r = new Random();
            string NewID = "T" + r.Next(1000, 5000);
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
                            //  AadObjectId = user.AadId,
                            //Id = turnContext.Activity.From.Id,
                            //Name = user.Name
                        }
                    },
                TenantId = turnContext.Activity.Conversation.TenantId,
            };

            await ((BotFrameworkAdapter)turnContext.Adapter).CreateConversationAsync(teamsChannelId, serviceUrl, credentials, conversationParameters,
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
            UserDetailsRepository userDetailsDataRepository = new UserDetailsRepository(configuration);

            UserDetailsEntity manager = await userDetailsDataRepository.GeUserDetails(userEmail.ToLower());

            return manager ?? null;
        }

        public static async Task SendNotificationAsync(IConfiguration configuration, Attachment attachment, UserDetailsEntity userDetailsEntity)
        {
            await FetchTokenAsync(configuration);
            var createConversationUrl = $"{userDetailsEntity.ServiceUrl}v3/conversations";
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, createConversationUrl))
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    botAccessToken);

                var payloadString = "{\"bot\": { \"id\": \"28:" + configuration["MicrosoftAppId"] + "\"},\"isGroup\": false, \"tenantId\": \"" + userDetailsEntity.TenantId + "\", \"members\": [{\"id\": \""
                    + userDetailsEntity.UserUniqueID + "\"}]}";
                requestMessage.Content = new StringContent(payloadString, Encoding.UTF8, "application/json");

                var httpClient = new HttpClient();
                using (var sendResponse = await httpClient.SendAsync(requestMessage))
                {
                    if (sendResponse.StatusCode == HttpStatusCode.Created)
                    {
                        var jsonResponseString = await sendResponse.Content.ReadAsStringAsync();
                        dynamic resp = JsonConvert.DeserializeObject(jsonResponseString);
                        if (string.IsNullOrEmpty(userDetailsEntity.ConversationId))
                        {
                            userDetailsEntity.ConversationId = resp.id;
                        }

                        await SendNotificationAsync(userDetailsEntity, attachment);

                        var operation = TableOperation.InsertOrMerge(userDetailsEntity);
                        if (userDataRepository == null)
                        {
                            userDataRepository = new UserDetailsRepository(configuration);
                        }

                        await userDataRepository.Table.ExecuteAsync(operation);
                    }
                }
            }
        }

        private static async Task SendNotificationAsync(UserDetailsEntity userData, Attachment card)
        {
            var conversationUrl = $"{userData.ServiceUrl}v3/conversations/{userData.ConversationId}/activities";
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, conversationUrl))
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    botAccessToken);

                var attachmentJsonString = JsonConvert.SerializeObject(card);

                var messageString = "{ \"type\": \"message\", \"attachments\": [ " + attachmentJsonString + " ], \"channelData\": { \"notification\": { \"alert\": true } } }";
                requestMessage.Content = new StringContent(messageString, Encoding.UTF8, "application/json");
                var httpClient = new HttpClient();
                using (var sendResponse = await httpClient.SendAsync(requestMessage))
                {
                    if (sendResponse.StatusCode == HttpStatusCode.Created)
                    {
                        return;
                    }

                    if (sendResponse.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        var random = new Random();
                        await Task.Delay(random.Next(500, 1500));
                    }
                }
            }
        }

        private static async Task FetchTokenAsync(
           IConfiguration configuration)
        {
            var values = new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" },
                    { "client_id", configuration["MicrosoftAppId"] },
                    { "client_secret", configuration["MicrosoftAppPassword"] },
                    { "scope", "https://api.botframework.com/.default" },
                };
            var content = new FormUrlEncodedContent(values);
            var httpClient = new HttpClient();
            using (var tokenResponse = await httpClient.PostAsync("https://login.microsoftonline.com/botframework.com/oauth2/v2.0/token", content))
            {
                if (tokenResponse.StatusCode == HttpStatusCode.OK)
                {
                    var accessTokenContent = await tokenResponse.Content.ReadAsStringAsync(); //JsonConvert.DeserializeObject<AccessTokenResponse>(tokenResponse.Content.ToString());// await .<AccessTokenResponse>();
                    var resp = JsonConvert.DeserializeObject<AccessTokenResponse>(accessTokenContent);

                    botAccessToken = resp.AccessToken;

                    var expiresInSeconds = 121;

                    // If parsing fails, out variable is set to 0, so need to set the default
                    if (!int.TryParse(resp.ExpiresIn, out expiresInSeconds))
                    {
                        expiresInSeconds = 121;
                    }

                    // Remove two minutes in order to have a buffer amount of time.
                }
                else
                {
                    throw new Exception("Error fetching bot access token.");
                }
            }
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
