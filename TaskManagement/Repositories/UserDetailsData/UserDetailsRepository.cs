using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagement.Repositories.UserDetailsData
{
    public class UserDetailsRepository : BaseRepository<UserDetailsEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserDataRepository"/> class.
        /// </summary>
        /// <param name="configuration">Represents the application configuration.</param>
        /// <param name="isFromAzureFunction">Flag to show if created from Azure Function.</param>
        public UserDetailsRepository(IConfiguration configuration, bool isFromAzureFunction = false)
            : base(
                configuration,
                PartitionKeyNames.UserDetailsDataTable.TableName,
                PartitionKeyNames.UserDetailsDataTable.UserDetailsDataPartition,
                isFromAzureFunction)
        {

        }

        public async Task<UserDetailsEntity> GeUserDetails(string emailId)
        {
            var allRows = await this.GetAllAsync(PartitionKeyNames.UserDetailsDataTable.TableName);
            UserDetailsEntity userDetailsEntity = allRows.Where(c => c.EmailId == emailId).FirstOrDefault();
            return userDetailsEntity;
        }

        public async Task SaveUserDetailsAsync(
            ITurnContext<IConversationUpdateActivity> turnContext)
        {
            var userDataEntity = await ParseUserData(turnContext);
            var allUsers = await GetUserDataDictionaryAsync();

            if (userDataEntity != null && !allUsers.ContainsKey(userDataEntity.AadId))
            {
                await this.CreateOrUpdateAsync(userDataEntity);
            }
        }

        private static async Task<UserDetailsEntity> ParseUserData(ITurnContext<IConversationUpdateActivity> turnContext)
        {
            var rowKey = turnContext.Activity?.From?.AadObjectId;
            if (rowKey != null)
            {
                var userDataEntity = new UserDetailsEntity
                {
                    PartitionKey = PartitionKeyNames.UserDetailsDataTable.UserDetailsDataPartition,
                    RowKey = turnContext.Activity?.From?.AadObjectId,
                    AadId = turnContext.Activity?.From?.AadObjectId,
                    UserUniqueID = turnContext.Activity?.From?.Id,
                    ConversationId = turnContext.Activity?.Conversation?.Id,
                    ServiceUrl = turnContext.Activity?.ServiceUrl,
                    TenantId = turnContext.Activity?.Conversation?.TenantId,
                    Name = await GetUserName(turnContext),
                    EmailId = await GetUserEmailId(turnContext),
                };

                return userDataEntity;
            }

            return null;
        }

        private static async Task<string> GetUserEmailId(ITurnContext<IConversationUpdateActivity> turnContext)
        {
            // Fetch the members in the current conversation
            try
            {
                IConnectorClient connector = turnContext.TurnState.Get<IConnectorClient>();
                // ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl)  );

                var members = await connector.Conversations.GetConversationMembersAsync(turnContext.Activity.Conversation.Id);
                return AsTeamsChannelAccounts(members).FirstOrDefault(m => m.Id == turnContext.Activity.From.Id).UserPrincipalName;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        private static async Task<string> GetUserName(ITurnContext<IConversationUpdateActivity> turnContext)
        {
            // Fetch the members in the current conversation
            try
            {
                IConnectorClient connector = turnContext.TurnState.Get<IConnectorClient>();
                // ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl)  );

                var members = await connector.Conversations.GetConversationMembersAsync(turnContext.Activity.Conversation.Id);
                return AsTeamsChannelAccounts(members).FirstOrDefault(m => m.Id == turnContext.Activity.From.Id).Name;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public static IEnumerable<TeamsChannelAccount> AsTeamsChannelAccounts(IEnumerable<ChannelAccount> channelAccountList)
        {
            foreach (ChannelAccount channelAccount in channelAccountList)
            {
                yield return JObject.FromObject(channelAccount).ToObject<TeamsChannelAccount>();
            }
        }

        public async Task SaveAllUserDetailsInTeams(
            ITurnContext<IConversationUpdateActivity> turnContext,
            IEnumerable<TeamsChannelAccount> members
            )
        {
            var allUsers = await GetUserDataDictionaryAsync();
            foreach (var member in members)
            {
                if (!allUsers.ContainsKey(member.AadObjectId))
                {
                    var userDetailEntity = this.ParseUserData(
                        member,
                        turnContext.Activity.ServiceUrl,
                        turnContext.Activity.Conversation.TenantId);
                    if (userDetailEntity != null)
                    {
                        await this.CreateOrUpdateAsync(userDetailEntity);
                    }
                }
            }
        }

        private UserDetailsEntity ParseUserData(TeamsChannelAccount member, string serviceUrl, string tenantId)
        {
            var rowKey = member.AadObjectId;
            if (rowKey != null)
            {
                var userDataEntity = new UserDetailsEntity
                {
                    PartitionKey = PartitionKeyNames.UserDetailsDataTable.UserDetailsDataPartition,
                    RowKey = member.AadObjectId,
                    AadId = member.AadObjectId,
                    UserUniqueID = member.Id,
                    ConversationId = string.Empty,
                    ServiceUrl = serviceUrl,
                    TenantId = tenantId,
                    Name = member.Name,
                    EmailId = member.Email,
                };

                return userDataEntity;
            }

            return null;
        }

        private async Task<Dictionary<string, UserDetailsEntity>> GetUserDataDictionaryAsync()
        {
            var userDataEntities = await this.GetAllAsync();
            var alluser = new Dictionary<string, UserDetailsEntity>();
            foreach (var userDataEntity in userDataEntities)
            {
                alluser.Add(userDataEntity.AadId, userDataEntity);
            }

            return alluser;
        }
    }
}
