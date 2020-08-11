using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManagement.Model;

namespace TaskManagement.Helper
{
    public class ChatMessageParser : MessageParserBase
    {
        private readonly List<MessageParserBase> parsers = new List<MessageParserBase>()
        {
            new ChannelMessageParser(),
        };

        public override Question Parse(ChatMessage message)
        {
            foreach (var parser in parsers)
            {
                var question = parser.Parse(message);
                if (question != null)
                    return question;
            }

            return base.Parse(message);
        }
    }
}
