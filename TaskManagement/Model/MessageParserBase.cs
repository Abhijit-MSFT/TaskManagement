using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagement.Model
{
    public class MessageParserBase
    {
        public string ForumName { get; set; }
        public string ForumDisplayName { get; set; }
        public virtual Question Parse(ChatMessage message)
        {
            var question = new Question
            {
                PostedDate = message.CreatedDateTime.Value.UtcDateTime.
                                   ToString("s", CultureInfo.InvariantCulture)
                //ToString("M/d/yyyy hh:mm tt", CultureInfo.InvariantCulture)
            };
            question.messageId = message.Id;
            var webUrl = message.WebUrl;
            if (string.IsNullOrEmpty(webUrl))
                webUrl = message.AdditionalData["linkToMessage"]?.ToString();
            if (!string.IsNullOrEmpty(webUrl))
            {
                var parts = System.Web.HttpUtility.UrlDecode(webUrl).Split('/').ToList();
                var index = parts.IndexOf("message");
                question.channelId = parts[index + 1];
            }
            question.Body = $"<a href='{webUrl}'>Deeplink to question in Microsoft Teams.</a><br/>";
            return question;
        }
    }
}
