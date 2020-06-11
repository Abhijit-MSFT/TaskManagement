using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
        public ActionResult Index()
        {
            return View();
        }


        [Route("hello")]
        public ActionResult Hello()
        {
            return View("Index");
        }

        //[Route("createNewTask/{turnContext}")]
        [Route("createNewTask/{titleFromPayload}")]
        public async Task<ActionResult> CreateNewTask(string titleFromPayload)
        {

            PageLoadData pageLoadData = await DBHelper.GetPageLoadDataAsync(_configuration);
            Common common = new Common(_configuration);
            string taskId = common.GetNewTaskID();
            ViewBag.newTaskID = taskId;
            ViewBag.description = titleFromPayload;
            return View(pageLoadData);
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

            TaskInfo taskInfo = new TaskInfo
            {
                taskNumber = taskdataEntity.TaskName,
                taskAssignedTo = taskdataEntity.TaskAssignedTo,
                status = taskdataEntity.TaskStatus,
                priority = taskdataEntity.TaskPriority,
                title = taskdataEntity.TaskTitle,
                description = taskdataEntity.TaskDescription,
                startDate = taskdataEntity.TaskStartDate,
                dueDate = taskdataEntity.TaskDueDate,
                attachementURL = taskAttachementsEntity.AttachementURL,
                subscribers = taskdataEntity.Subscribers.ToList(),
                dependentOn = taskdataEntity.Dependencies.ToList(),
                blocks = taskdataEntity.Blocks.ToList(),
                activity = taskActivityEntity
            };
            
            return View(taskInfo);
        }



        

        [Route("configure")]
        public ActionResult Configure()
        {
            return View();
        }
    
       

    }
}