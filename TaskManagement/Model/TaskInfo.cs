using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;


namespace TaskManagement.Model
{
    public class TaskInfo
    {
        public Guid taskID { get; set; }
        public string taskName{ get; set; }
        public string title { get; set; }
        public string taskNumber { get; set; }
        public string taskCreatedBy { get; set; }
        public string taskCreatedByEmail { get; set; }
        public string taskAssignedTo { get; set; }
        public string subscribers { get; set; }
        public string status { get; set; }
        public DateTime startDate { get; set; }
        public string priority { get; set; }
        public DateTime dueDate { get; set; }
        public string description { get; set; }
        public string depemdemtOn { get; set; }
        public string blocks { get; set; }
        public string messageID { get; set; }
        public string attachementURL { get; set; }
        public string activityComment { get; set; }
        public string action { get; set; }
        public string type { get; set; }
        public Guid attachementID { get; set; }
        public Guid subscriberID { get; set; }
        public Guid dependencyID { get; set; }
        public Guid taskActivityID { get; set; }
    }



    public class UserfeedbackInfo
    {
        public int feedbackId { get; set; }
        public Guid reflectionId { get; set; }
        public string action { get; set; }
        public string type { get; set; }
        public string userName { get; set; }
        public string emailId { get; set; }
        public string messageId { get; set; }
    }

    public class MessageDetails
    {
        public string messageid { get; set; }
    }

    public class ReflctionData
    {
        public Data data { get; set; }
    }

    public class Data
    {
        public string URL { get; set; }
        public string type { get; set; }
    }



    public class TaskModuleActionHelper
    {
        public class AdaptiveCardValue<T>
        {
            [JsonProperty("msteams")]
            public object Type { get; set; } = JsonConvert.DeserializeObject("{\"type\": \"task/fetch\" }");
            [JsonProperty("data")]
            public T Data { get; set; }
        }
    }

    public class ActionDetails
    {
        public string type { get; set; }
        public string ActionType { get; set; }
    }
    public class TaskModuleActionDetails : ActionDetails
    {
        public string URL { get; set; }
        public string Title { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
    }

    public class Responses
    {
        public string Createdby { get; set; }
        public string QuestionTitle { get; set; }

        public List<OptionResponse> OptionResponses { get; set; }

    }
    public class OptionResponse
    {
        public int Width { get; set; }
        public string Color { get; set; }

        public string Description { get; set; }
    }


}
