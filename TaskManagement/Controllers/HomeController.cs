using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Configuration;
using TaskManagement.Helper;
using TaskManagement.Model;
using TaskManagement.Repositories.TaskActivityData;
using TaskManagement.Repositories.TaskAttachementsData;
using TaskManagement.Repositories.TaskDependencyData;
using TaskManagement.Repositories.TaskDetailsData;
using TaskManagement.Repositories.TaskSubscribersData;

namespace TaskManagement.Controllers
{

    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Route("")]
        public async Task<ActionResult> Index()
        {
            TaskDataRepository taskDataRepository = new TaskDataRepository(_configuration);
            List<TaskDataEntity> taskDataEntity = await taskDataRepository.GetUserTasksAsync("Gousia Begum");
            return View(taskDataEntity);
        }


        [Route("hello")]
        public ActionResult Hello()
        {
            return View("Index");
        }

        //[Route("createNewTask/{turnContext}")]
        [Route("createNewTask/{titleFromPayload}")]
        public async Task<ActionResult> CreateNewTask(string titleFromPayload, string createType, string parentTaskId)
        {

            PageLoadData pageLoadData = await DBHelper.GetPageLoadDataAsync(_configuration);
            Common common = new Common(_configuration);
            string newTaskId = common.GetNewTaskID();
            ViewBag.newTaskID = newTaskId;


            var taskList = (await DBHelper.GetPageLoadDataAsync(_configuration)).ListofTaskIDs;
            TaskInfo taskInfo = new TaskInfo
            {
                assignedToList = this.GetListOfUser(),
                statusList = this.GetStatusList(),
                priorityList = this.GetPriorityList(),
                description = titleFromPayload ?? "",
                startDate = DateTime.Today,
                dueDate = DateTime.Today,
                subscribersList = this.GetListOfUser(),
                dependentOnList = this.GetTaskListSelectItems(taskList),
                blocksList = this.GetTaskListSelectItems(taskList),
            };

            if (createType == "Depends on")
            {
                taskInfo.dependentOn = new List<string>() { parentTaskId };
                ViewBag.action = createType;
                ViewBag.parentTaskName = parentTaskId;

            }

            if (createType == "Blocks")
            {
                taskInfo.blocks = new List<string>() { parentTaskId };
                ViewBag.action = createType;
                ViewBag.parentTaskName = parentTaskId;
            }

            return View(taskInfo);
        }

        [Route("createNewTask/{createType}/{taskId}")]
        public async Task<ActionResult> CreateNewTaskFromOld(string createType, string taskId)
        {

            PageLoadData pageLoadData = await DBHelper.GetPageLoadDataAsync(_configuration);
            Common common = new Common(_configuration);
            string newTaskId = common.GetNewTaskID();
            ViewBag.newTaskID = newTaskId;
            ViewBag.description = "";
            var taskList = (await DBHelper.GetPageLoadDataAsync(_configuration)).ListofTaskIDs;
            TaskInfo taskInfo = new TaskInfo
            {
                assignedToList = this.GetListOfUser(),
                statusList = this.GetStatusList(),
                priorityList = this.GetPriorityList(),
                description = "",
                startDate = DateTime.Today,
                dueDate = DateTime.Today,
                subscribersList = this.GetListOfUser(),
                dependentOnList = this.GetTaskListSelectItems(taskList),
                blocksList = this.GetTaskListSelectItems(taskList),
            };
            return View(taskInfo);
        }

        [Route("editTask/{taskId}")]
        public async Task<ActionResult> EditTask(Guid taskId)
        {
            TaskDataRepository taskDataRepository = new TaskDataRepository(_configuration);
            TaskAttachementsRepository taskAttachementsRepository = new TaskAttachementsRepository(_configuration);
            TaskActivityRepository taskActivityRepository = new TaskActivityRepository(_configuration);

            TaskDataEntity taskdataEntity = await taskDataRepository.GetTaskDetailsByTaskIDAsync(taskId);
            TaskAttachementsEntity taskAttachementsEntity = await taskAttachementsRepository.GetTaskAttachementDetailsByTaskIDAsync(taskId);
            List<TaskActivityEntity> taskActivityEntity = await taskActivityRepository.GetTaskActivityDetailsByTaskIDAsync(taskId);
            var sortedActivityList = taskActivityEntity.OrderByDescending(x => x.Timestamp).ToList();
            var taskList = (await DBHelper.GetPageLoadDataAsync(_configuration)).ListofTaskIDs;
            TaskInfo taskInfo = new TaskInfo
            {
                taskID = taskId,
                taskNumber = taskdataEntity.TaskName,
                taskAssignedTo = taskdataEntity.TaskAssignedTo,
                assignedToList = this.GetListOfUser(),
                status = taskdataEntity.TaskStatus,
                statusList = this.GetStatusList(),
                priority = taskdataEntity.TaskPriority,
                priorityList = this.GetPriorityList(),
                title = taskdataEntity.TaskTitle,
                description = taskdataEntity.TaskDescription,
                startDate = taskdataEntity.TaskStartDate,
                dueDate = taskdataEntity.TaskDueDate,
                attachementURL = taskAttachementsEntity.AttachementURL,
                subscribers = taskdataEntity.Subscribers.ToList(),
                subscribersList = this.GetListOfUser(),
                dependentOn = taskdataEntity.Dependencies.ToList(),
                //ListOfDependsOnAndBlocks = this.GetTaskListSelectItems(taskList),
                dependentOnList = this.GetTaskListSelectItems(taskList),
                blocks = taskdataEntity.Blocks.ToList(),
                blocksList = this.GetTaskListSelectItems(taskList),
                activity = sortedActivityList,
            };
            return View(taskInfo);
        }

        private IEnumerable<SelectListItem> GetTaskListSelectItems(Dictionary<string, string> taskList)
        {
            var list = new List<SelectListItem>();
            foreach (var keyValue in taskList)
            {
                list.Add(new SelectListItem() { Value = keyValue.Key, Text = keyValue.Key + " - " + keyValue.Value });
            }
            return list;
        }

        private IEnumerable<SelectListItem> GetPriorityList()
        {
            return new List<SelectListItem>()
            {
               new SelectListItem(){Value = "low", Text ="Low" },
               new SelectListItem(){Value = "medium", Text ="Medium" },
               new SelectListItem(){Value = "high", Text ="High" },
            };
        }

        private IEnumerable<SelectListItem> GetStatusList()
        {
            return new List<SelectListItem>()
            {
               new SelectListItem(){Value = "open", Text ="Open" },
               new SelectListItem(){Value = "pending", Text ="pending" },
               new SelectListItem(){Value = "resolved", Text ="Resolved" },
               new SelectListItem(){Value = "closed", Text ="Closed" },
            };
        }

        private IEnumerable<SelectListItem> GetListOfUser()
        {
            return new List<SelectListItem>()
            {
               new SelectListItem(){Value = "Abhijit Jodbhavi", Text ="Abhijit Jodbhavi" },
               new SelectListItem(){Value = "Gousia Begum", Text ="Gousia Begum" },
               new SelectListItem(){Value = "Trinetra Kumar", Text ="Trinetra Kumar" },
               new SelectListItem(){Value =  "Subhasish Pani", Text = "Subhasish Pani" }
            };
        }

        [Route("configure")]
        public ActionResult Configure()
        {
            return View();
        }
    }
}