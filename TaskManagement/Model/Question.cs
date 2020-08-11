using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagement.Model
{
    public class Question
    {
        public string UniqueId { get; set; }
        public int questionId { get; set; }
        public string messageId { get; set; }
        public string channelId { get; set; }
        public string Title { get; set; }
        public string PostedDate { get; set; }
        public string QuestionType { get; set; }
        public string QuestionSubType { get; set; }
        public string Forum { get; set; }
        public string Status { get; set; }
        public string Tags { get; set; }
        public string ChangedDate { get; set; }
        public string Module { get; set; }
        public string AssignedTo { get; set; }
        public string Body { get; set; }
        public string Comments { get; set; }
        public bool IsNewQuestion { get; set; }
    }
}
