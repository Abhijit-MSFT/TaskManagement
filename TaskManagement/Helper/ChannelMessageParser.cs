using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManagement.Model;

namespace TaskManagement.Helper
{
    public class ChannelMessageParser : MessageParserBase
    {
        public ChannelMessageParser()
        {
            
        }
        

        public override Question Parse(ChatMessage message)
        {
            if (string.IsNullOrEmpty(message.Subject))
                return null;

            var question = base.Parse(message);

            if (message.Subject.Contains("RE: ") ||
           message.Subject.Contains("Re: ") || message.Subject.Contains(":"))
                question.IsNewQuestion = false;
            else
                question.IsNewQuestion = true;

            //question.Title = message.Subject;
            //question.UniqueId = CommonHelper.GetConanicalMailSubject(message.Subject)?.ToLower();
            //question.Body += message.Body.Content;
            //question.Forum = ForumName;

            return question;
        }
    }
}
