using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace TaskManagement.Helper
{
    public class SubscriptionManager : BackgroundService
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        public static readonly Dictionary<string, Subscription> Subscriptions = new Dictionary<string, Subscription>();
        public SubscriptionManager(IConfiguration config, ILogger<SubscriptionManager> logger)
        {
            _config = config;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await InitializeAllSubscription();
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(4), stoppingToken).ConfigureAwait(false);
                await this.CheckSubscriptions().ConfigureAwait(false); ;
            }
        }
        private async Task InitializeAllSubscription()
        {            
            var teamId = _config["NotificationSetting:TeamId"];
            await CreateNewSubscription(teamId, _config["NotificationSetting:TestChannelID"]);
            await this.CheckSubscriptions().ConfigureAwait(false);            
        }
        private async Task<Subscription> CreateNewSubscription(string teamId, string channelId)
        {
            _logger.LogWarning($"CreateNewSubscription-start: {teamId}-{channelId}");
            if (string.IsNullOrEmpty(teamId) || string.IsNullOrEmpty(channelId))
                return null;
            var resource = $"teams/{teamId}/channels/{channelId}/messages";
            return await CreateSubscriptionWithResource(resource);
        }
        private async Task<Subscription> CreateSubscriptionWithResource(string resource)
        {
            if (string.IsNullOrEmpty(resource))
                return null;
            var graphServiceClient = GetGraphClient();
            if (Subscriptions.Any(s => s.Value.Resource == resource))
                return null;
            IGraphServiceSubscriptionsCollectionPage existingSubscriptions = null;
            try
            {
                existingSubscriptions = await graphServiceClient
                          .Subscriptions
                          .Request().
                          GetAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"CreateNewSubscription-ExistingSubscriptions-Failed: {resource}");
                return null;
            }
            var notificationUrl = _config["BaseUrl"] + "/api/notifications";
            var existingSubscription = existingSubscriptions.FirstOrDefault(s => s.Resource == resource);
            if (existingSubscription != null && existingSubscription.NotificationUrl != notificationUrl)
            {
                _logger.LogWarning($"CreateNewSubscription-ExistingSubscriptionFound: {resource}");
                DeleteSubscription(existingSubscription);
                existingSubscription = null;
            }
            if (existingSubscription == null)
            {
                var sub = new Subscription
                {
                    Resource = resource,
                    ChangeType = "created",
                    NotificationUrl = notificationUrl,
                    ClientState = Guid.NewGuid().ToString(),
                    ExpirationDateTime = DateTime.UtcNow + new TimeSpan(days: 0, hours: 0, minutes: 10, seconds: 0),
                    IncludeProperties = false,
                    LifecycleNotificationUrl = _config["BaseUrl"] + "/api/webhookLifecyle",
                    AdditionalData = new Dictionary<string, object>()
                    {
                        ["includeResourceData"] = true,
                        ["encryptionCertificate"] = _config["SelfSignedCert"],
                        ["encryptionCertificateId"] = "readchannelmessage",
                    }
                };
                try
                {
                    existingSubscription = await graphServiceClient
                              .Subscriptions
                              .Request()
                              .AddAsync(sub);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"CreateNewSubscription-Failed: {resource}");
                    return null;
                }
            }
            Subscriptions[existingSubscription.Id] = existingSubscription;
            _logger.LogWarning($"Subscription Created for TeamId: {resource}");
            return existingSubscription;
        }
        private async Task CheckSubscriptions()
        {
            _logger.LogWarning($"Checking subscriptions {DateTime.Now.ToString("h:mm:ss.fff")}");
            if (Subscriptions.Count != 3)
            {
                _logger.LogWarning($"CheckSubscriptions-Number of subscription={Subscriptions.Count}");
                // Possible failure.
                InitializeAllSubscription().RunSynchronously();
            }
            foreach (var subscription in Subscriptions)
            {
                // if the subscription expires in the next 5 min, renew it
                if (subscription.Value.ExpirationDateTime < DateTime.UtcNow.AddMinutes(2))
                {
                    RenewSubscription(subscription.Value);
                }
            }
        }
        private async void RenewSubscription(Subscription subscription)
        {
            _logger.LogWarning($"Current subscription: {subscription.Id}, Expiration: {subscription.ExpirationDateTime}");
            var graphServiceClient = GetGraphClient();
            var newSubscription = new Subscription
            {
                ExpirationDateTime = DateTime.UtcNow.AddMinutes(10)
            };
            try
            {
                await graphServiceClient
                     .Subscriptions[subscription.Id]
                     .Request()
                     .UpdateAsync(newSubscription);
                subscription.ExpirationDateTime = newSubscription.ExpirationDateTime;
                _logger.LogWarning($"Renewed subscription: {subscription.Id}, New Expiration: {subscription.ExpirationDateTime}");
            }
            catch (Microsoft.Graph.ServiceException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogError(ex, $"HttpStatusCode.NotFound : Creating new subscription : {subscription.Id}");
                    // Try and create new resource.
                    Subscriptions.Remove(subscription.Id);
                    await CreateSubscriptionWithResource(subscription.Resource);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Update Subscription Failed: {subscription.Id}");
            }
        }
        private async void DeleteSubscription(Subscription subscription)
        {
            _logger.LogWarning($"Current subscription: {subscription.Id}, Expiration: {subscription.ExpirationDateTime}");
            var graphServiceClient = GetGraphClient();
            try
            {
                await graphServiceClient
                     .Subscriptions[subscription.Id]
                     .Request()
                     .DeleteAsync();
                _logger.LogWarning($"Deleted subscription: {subscription.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Delete Subscription Failed: {subscription.Id}");
            }
        }
        private GraphServiceClient GetGraphClient()
        {
            var graphClient = new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) =>
            {
                // get an access token for Graph
                var accessToken = GetAccessToken().Result;
                requestMessage
                          .Headers
                          .Authorization = new AuthenticationHeaderValue("bearer", accessToken);
                return Task.FromResult(0);
            }));
            return graphClient;
        }
        private async Task<string> GetAccessToken()
        {
            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(_config["MicrosoftAppId"])
              .WithClientSecret(_config["MicrosoftAppPassword"])
              .WithAuthority($"https://login.microsoftonline.com/{_config["TenantId"]}")
              .WithRedirectUri("https://daemon")
              .Build();
            string[] scopes = new string[] { "https://graph.microsoft.com/.default" };
            var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();
            return result.AccessToken;
        }
    }
}
