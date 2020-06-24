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
using TaskManagement.Repositories.UserDetailsData;

namespace TaskManagement.Controllers
{

    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly TaskDataRepository _taskDataRepository;
        private readonly TaskAttachementsRepository _taskAttachementsRepository;
        private readonly TaskActivityRepository _taskActivityRepository;        
        
        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
            _taskDataRepository = new TaskDataRepository(_configuration);
            _taskAttachementsRepository = new TaskAttachementsRepository(_configuration);
            _taskActivityRepository = new TaskActivityRepository(_configuration);
        }

        [Route("")]
        public async Task<ActionResult> Index()
        {            
            List<TaskDataEntity> taskDataEntity = await _taskDataRepository.GetUserTasksAsync("Gousia Begum");
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
            var userList = await DBHelper.GetListOfUser(_configuration);
            TaskInfo taskInfo = new TaskInfo
            {
                assignedToList = userList,
                statusList = this.GetStatusList(),
                priorityList = this.GetPriorityList(),
                description = titleFromPayload ?? "",
                startDate = DateTime.Today,
                dueDate = DateTime.Today,
                subscribersList = userList,
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
        public async Task<ActionResult> CreateNewTaskFromOld()
        {
            PageLoadData pageLoadData = await DBHelper.GetPageLoadDataAsync(_configuration);
            Common common = new Common(_configuration);
            string newTaskId = common.GetNewTaskID();
            ViewBag.newTaskID = newTaskId;
            ViewBag.description = "";
            var taskList = (await DBHelper.GetPageLoadDataAsync(_configuration)).ListofTaskIDs;
            var userList = await DBHelper.GetListOfUser(_configuration);
            var listofTasks = this.GetTaskListSelectItems(taskList);
            TaskInfo taskInfo = new TaskInfo
            {
                assignedToList = userList,
                statusList = this.GetStatusList(),
                priorityList = this.GetPriorityList(),
                description = "",
                startDate = DateTime.Today,
                dueDate = DateTime.Today,
                subscribersList = userList,
                dependentOnList = listofTasks,
                blocksList = listofTasks,
            };
            return View(taskInfo);
        }

        [Route("editTask/{taskId}")]
        public async Task<ActionResult> EditTask(Guid taskId)
        {
            TaskDataEntity taskdataEntity = await _taskDataRepository.GetTaskDetailsByTaskIDAsync(taskId);
            TaskAttachementsEntity taskAttachementsEntity = await _taskAttachementsRepository.GetTaskAttachementDetailsByTaskIDAsync(taskId);
            List<TaskActivityEntity> taskActivityEntity = await _taskActivityRepository.GetTaskActivityDetailsByTaskIDAsync(taskId);
            var sortedActivityList = taskActivityEntity.OrderByDescending(x => x.Timestamp).ToList();
            var taskList = (await DBHelper.GetPageLoadDataAsync(_configuration)).ListofTaskIDs;
            var userList = await DBHelper.GetListOfUser(_configuration);
            var listofTasks = this.GetTaskListSelectItems(taskList);
            TaskInfo taskInfo = new TaskInfo
            {
                taskID = taskId,
                taskNumber = taskdataEntity.TaskName,
                taskAssignedTo = taskdataEntity.TaskAssignedTo,
                assignedToList = userList,
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
                subscribersList = userList,
                dependentOn = taskdataEntity.Dependencies.ToList(),                
                dependentOnList = listofTasks,
                blocks = taskdataEntity.Blocks.ToList(),
                blocksList = listofTasks,
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
               new SelectListItem(){Value = "Low", Text ="Low" },
               new SelectListItem(){Value = "Medium", Text ="Medium" },
               new SelectListItem(){Value = "High", Text ="High" },
            };
        }

        private IEnumerable<SelectListItem> GetStatusList()
        {
            return new List<SelectListItem>()
            {
               new SelectListItem(){Value = "Open", Text ="Open" },
               new SelectListItem(){Value = "Pending", Text ="Pending" },
               new SelectListItem(){Value = "Resolved", Text ="Resolved" },
               new SelectListItem(){Value = "Closed", Text ="Closed" },
            };
        }



        [Route("configure")]
        public ActionResult Configure()
        {
            return View();
        }
    }
}