using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaskManagement.Repositories.TaskActivityData;

namespace TaskManagement.Model
{
    public class TaskInfo
    {
        public Guid taskID { get; set; }
        public string taskName { get; set; }

        public string title { get; set; }
        public string taskNumber { get; set; }
        public string taskCreatedBy { get; set; }
        public string taskCreatedByEmail { get; set; }
        public string taskAssignedTo { get; set; }
        public IEnumerable<SelectListItem> assignedToList { get; set; }

        public List<string>? subscribers { get; set; }
        public IEnumerable<SelectListItem> subscribersList { get; set; }

        public string status { get; set; }
        public IEnumerable<SelectListItem> statusList { get; set; }
        public List<string> allDependentTaskIDs { get; set; }

        public List<string> allBlocksTaskIDs { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime startDate { get; set; }
        public string priority { get; set; }
        public IEnumerable<SelectListItem> priorityList { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime dueDate { get; set; }
        public string description { get; set; }
        public List<string>? dependentOn { get; set; }
        public IEnumerable<SelectListItem> dependentOnList { get; set; }
        public List<string>? blocks { get; set; }
        public IEnumerable<SelectListItem> blocksList { get; set; }

        public string messageID { get; set; }
        public string? attachement { get; set; }
        [NotMapped]
        public IFormFile? attachements { get; set; }
        public string? attachementURL { get; set; }
        public List<TaskActivityEntity>? activity { get; set; }
        public string activityComment { get; set; }
        public string action { get; set; }
        public string type { get; set; }
        public Guid attachementID { get; set; }
        public Guid subscriberID { get; set; }
        public string ParentTaskName { get; set; }

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

    public class TaskData
    {
        public Data data { get; set; }
    }

    public class Data
    {
        public string URL { get; set; }
        public string type { get; set; }
        public string CreateType { get; set; }
        public string TaskId { get; set; }

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
        public string CreateType { get; set; }
        public string TaskId { get; set; }

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
