using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TaskManagement.Helper;
using TaskManagement.Model;
using TaskManagement.Model.NotificationModels;
using Microsoft.Graph;

namespace TaskManagement.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {

        private readonly IConfiguration _config;
        private readonly MessageParserBase _messageParser;
        private readonly string _rootPath;
        private readonly ILogger _logger;
        private readonly static ConcurrentDictionary<string, bool> _processedMessage = new ConcurrentDictionary<string, bool>(2, 64);

        [Obsolete]
        public NotificationsController(IConfiguration config, MessageParserBase messageParser, IHostingEnvironment hostingEnvironment, ILogger<NotificationsController> logger)
        {
            _config = config;
            _messageParser = messageParser;
            _rootPath = hostingEnvironment.WebRootPath;
            _logger = logger;
        }

        [HttpGet]
        public async Task<JsonResult> Get()
        {
            //return Json(SubscriptionManager.Subscriptions);
            return null;
        }


        // Callback
        [Route("/api/webhookLifecyle")]
        [HttpPost]
        public ActionResult WebhookLifecyle([FromQuery]string validationToken = null)
        {
            if (validationToken != null)
            {
                // Ack the webhook subscription
                return Ok(validationToken);
            }
            else
            {
                
                return null;
            }
        }

        public async Task<ActionResult<string>> Post([FromQuery]string validationToken = null)
        {
            // handle validation
            if (!string.IsNullOrEmpty(validationToken))
            {
                _logger.LogInformation($"Received Token: '{validationToken}'");
                return Ok(validationToken);
            }



            // handle notifications
            using (StreamReader reader = new StreamReader(Request.Body))
            {
                string content = await reader.ReadToEndAsync();



                Console.WriteLine(content);



                var notifications = JsonConvert.DeserializeObject<Model.NotificationModels.Notifications>(content);



                // var filePath = Path.Combine(_rootPath, _config["CertFilePath"]);
                // var certData = System.IO.File.ReadAllBytes(filePath);



                foreach (var notifi in notifications.Items)
                {
                    // Initialize with the private key that matches the encryptionCertificateId.
                    var data = DecryptionHelper.GetDecryptedContent(notifi.EncryptedContent,
                                                    _config["WEBSITE_LOAD_CERTIFICATES"]);



                    _logger.LogWarning("Received message: " + data);
                    var message = JsonConvert.DeserializeObject<ChatMessage>(data);




                    if (!_processedMessage.TryAdd(message.WebUrl, true))
                    {
                        _logger.LogWarning("Multiple request received: " + message.WebUrl);
                        _logger.LogWarning("_processedMessage.Count: " + _processedMessage.Count);



                        continue;
                    }



                    var question = _messageParser.Parse(message);



                    if (question.Forum == null) // Don't process if Forum is not populated.
                    {
                        _logger.LogWarning("Skipped message: " + data);
                        continue;
                    }
                }
            }



            return Ok();
        }

    }
}
