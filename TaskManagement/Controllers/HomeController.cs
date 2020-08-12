using AdaptiveCards;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManagement.Helper;
using TaskManagement.Model;
using TaskManagement.Repositories.TaskActivityData;
using TaskManagement.Repositories.TaskAttachementsData;
using TaskManagement.Repositories.TaskDetailsData;

namespace TaskManagement.Controllers
{

    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly CardHelper _cardHelper;
        private readonly TaskDataRepository _taskDataRepository;
        private readonly TaskAttachementsRepository _taskAttachementsRepository;
        private readonly TaskActivityRepository _taskActivityRepository;

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
            _cardHelper = new CardHelper(_configuration);
            _taskDataRepository = new TaskDataRepository(_configuration);
            _taskAttachementsRepository = new TaskAttachementsRepository(_configuration);
            _taskActivityRepository = new TaskActivityRepository(_configuration);
        }

        //[Route("Qtracker")]
        //public ActionResult QuestionTracker()
        //{
        //    var questionType = _configuration["QuestionType"];
        //    if (questionType != null)
        //    {
        //        ViewBag.QuestionType = questionType;
        //    }
        //    var questionSubType = _configuration["QuestionSubType"];
        //    var complexity = _configuration["Complexity"];           
        //    var module = _configuration["Module"];
        //    var subModule = _configuration["SubModule"];
        //    var assignedTo = _configuration["AssignedTo"];


        //    return View();
        //}

        [Route("")]
        public async Task<ActionResult> Index()
        {
            var url = _configuration["BaseUri"];
            if (!string.IsNullOrEmpty(url))
            {
                ViewBag.BaseURL = url;
            }

            List<TaskDataEntity> taskDataEntity = await _taskDataRepository.GetUserTasksAsync("v-varssl@microsoft.com");

            //string token = GetAccessToken().ToString();
            //string userId = "v-varssl@microsoft.com";
            //var pic = GetPhoto(token, userId);
            return View(taskDataEntity.OrderByDescending(o => o.TaskDueDate).ToList());
        }

        [Route("hello")]
        public ActionResult Hello()
        {
            return View("Index");
        }

        [HttpPost]
        [Route("SaveOrUpdateTask")]
        public async Task<ActionResult> SaveOrUpdateTask(TaskInfo taskInfo)
        {
            var userList = await DBHelper.GetListOfUser(_configuration);
            var allUser = await DBHelper.GetUserDataDictionaryAsync(_configuration);
            var name = userList.Where(e => e.Value == taskInfo.userName).Select(e => e.Text).FirstOrDefault();
            taskInfo.taskCreatedBy = name;
            taskInfo.taskCreatedByEmail = taskInfo.userName;
            taskInfo.allDependentTaskIDs = await _taskDataRepository.GetAllTaskIDsAndTitles(taskInfo.dependentOn);
            taskInfo.allBlocksTaskIDs = await _taskDataRepository.GetAllTaskIDsAndTitles(taskInfo.blocks);
            CardHelper cardhelper = new CardHelper(_configuration);
            var attPath = taskInfo.attachements;
            await DBHelper.SaveTaskInfo(taskInfo, _configuration);

            var adaptiveCard = cardhelper.TaskInformationCard(taskInfo);
            var cardAttachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = adaptiveCard,
            };
            var recipietSet = new HashSet<string>();
            foreach (var item in taskInfo.subscribers)
            {
                recipietSet.Add(item);
            }
            recipietSet.Add(taskInfo.taskAssignedTo);
            recipietSet.Add(taskInfo.taskCreatedByEmail);

            foreach (var recipient in recipietSet)
            {
                if (allUser.ContainsKey(recipient))
                {
                    await Common.SendNotificationAsync(_configuration, cardAttachment, allUser[recipient]);
                }
            }

            return View(taskInfo);
        }

        #region Get profile photo
        //[HttpGet("api/GetAccessTokenAsync")]
        //public async Task<string> GetAccessToken()
        //{
        //    try
        //    {
        //        string tenant = _configuration["Tenant"];
        //        string appId = _configuration["AppId"];
        //        string appSecret = _configuration["AppSecret"];
        //        string response = await POST($"https://login.microsoftonline.com/{tenant}/oauth2/v2.0/token",
        //                          $"grant_type=client_credentials&client_id={appId}&client_secret={appSecret}"
        //                          + "&scope=https%3A%2F%2Fgraph.microsoft.com%2F.default");

        //        string accessToken = JsonConvert.DeserializeObject<TaskManagement.Model.TokenResponse>(response).access_token;
        //        return accessToken;
        //    }
        //    catch (Exception ex)
        //    {

        //        return null;
        //    }
        //}

        //public async Task<string> POST(string url, string body)
        //{
        //    try
        //    {
        //        HttpClient httpClient = new HttpClient();

        //        var request = new HttpRequestMessage(HttpMethod.Post, url);
        //        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //        request.Content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");
        //        HttpResponseMessage response = await httpClient.SendAsync(request);
        //        string responseBody = await response.Content.ReadAsStringAsync();

        //        if (!response.IsSuccessStatusCode)
        //            throw new Exception(responseBody);
        //        return responseBody;
        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }

        //}

        //[HttpGet("ProfilePhoto")]
        //public async Task<string> GetPhoto(string token, string userid)
        //{
        //    token = "eyJ0eXAiOiJKV1QiLCJub25jZSI6IkNXazgwek5Nb18tcHF0blAtWVZKSC13bEhJTzdqS0RPY2dKeWZHY1FaRnMiLCJhbGciOiJSUzI1NiIsIng1dCI6Imh1Tjk1SXZQZmVocTM0R3pCRFoxR1hHaXJuTSIsImtpZCI6Imh1Tjk1SXZQZmVocTM0R3pCRFoxR1hHaXJuTSJ9.eyJhdWQiOiIwMDAwMDAwMy0wMDAwLTAwMDAtYzAwMC0wMDAwMDAwMDAwMDAiLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC83MmY5ODhiZi04NmYxLTQxYWYtOTFhYi0yZDdjZDAxMWRiNDcvIiwiaWF0IjoxNTk0OTY0MTcwLCJuYmYiOjE1OTQ5NjQxNzAsImV4cCI6MTU5NDk2ODA3MCwiYWNjdCI6MCwiYWNyIjoiMSIsImFpbyI6IkFVUUF1LzhRQUFBQWN6MVZIQUtSN0ZOeUp1bFMxR1JTeWxtck5MNVhkVXZWM3VPMDhKdEZpQ3hNQjAvdFh4T1l6WjZMTWZlZ212a3NJMzFrNkY4M0s0QjY0RGpBTUZvUVNnPT0iLCJhbXIiOlsid2lhIiwibWZhIl0sImFwcF9kaXNwbGF5bmFtZSI6IkdyYXBoIGV4cGxvcmVyIChvZmZpY2lhbCBzaXRlKSIsImFwcGlkIjoiZGU4YmM4YjUtZDlmOS00OGIxLWE4YWQtYjc0OGRhNzI1MDY0IiwiYXBwaWRhY3IiOiIwIiwiZGV2aWNlaWQiOiIxNTM2M2Q1Ny0xNzE0LTQyNGMtODMxOC1hYzNjNjM2ZjQzYzUiLCJmYW1pbHlfbmFtZSI6IlNTTE4iLCJnaXZlbl9uYW1lIjoiVmFyYXByYXNhZCIsImluX2NvcnAiOiJ0cnVlIiwiaXBhZGRyIjoiMTU3LjQ4LjE2Ni4yMDIiLCJuYW1lIjoiVmFyYXByYXNhZCBTU0xOIChaRU4zIElORk9TT0xVVElPTlMgUFJJVkFURSBMSU0pIiwib2lkIjoiZjIxOTFkNWItNjI0Ni00ZDdiLThjMTktNzJkY2E4M2I3OWYwIiwib25wcmVtX3NpZCI6IlMtMS01LTIxLTIxNDY3NzMwODUtOTAzMzYzMjg1LTcxOTM0NDcwNy0yNjI3MjY1IiwicGxhdGYiOiIzIiwicHVpZCI6IjEwMDMyMDAwQzQzMkI3RjYiLCJyaCI6IjAuQVJvQXY0ajVjdkdHcjBHUnF5MTgwQkhiUjdYSWk5NzUyYkZJcUsyM1NOcHlVR1FhQUVBLiIsInNjcCI6IkNhbGVuZGFycy5SZWFkV3JpdGUgQ29udGFjdHMuUmVhZFdyaXRlIERldmljZU1hbmFnZW1lbnRBcHBzLlJlYWRXcml0ZS5BbGwgRGV2aWNlTWFuYWdlbWVudENvbmZpZ3VyYXRpb24uUmVhZC5BbGwgRGV2aWNlTWFuYWdlbWVudENvbmZpZ3VyYXRpb24uUmVhZFdyaXRlLkFsbCBEZXZpY2VNYW5hZ2VtZW50TWFuYWdlZERldmljZXMuUHJpdmlsZWdlZE9wZXJhdGlvbnMuQWxsIERldmljZU1hbmFnZW1lbnRNYW5hZ2VkRGV2aWNlcy5SZWFkLkFsbCBEZXZpY2VNYW5hZ2VtZW50TWFuYWdlZERldmljZXMuUmVhZFdyaXRlLkFsbCBEZXZpY2VNYW5hZ2VtZW50UkJBQy5SZWFkLkFsbCBEZXZpY2VNYW5hZ2VtZW50UkJBQy5SZWFkV3JpdGUuQWxsIERldmljZU1hbmFnZW1lbnRTZXJ2aWNlQ29uZmlnLlJlYWQuQWxsIERldmljZU1hbmFnZW1lbnRTZXJ2aWNlQ29uZmlnLlJlYWRXcml0ZS5BbGwgRGlyZWN0b3J5LkFjY2Vzc0FzVXNlci5BbGwgRGlyZWN0b3J5LlJlYWRXcml0ZS5BbGwgRmlsZXMuUmVhZFdyaXRlLkFsbCBHcm91cC5SZWFkV3JpdGUuQWxsIElkZW50aXR5Umlza0V2ZW50LlJlYWQuQWxsIE1haWwuUmVhZFdyaXRlIE1haWxib3hTZXR0aW5ncy5SZWFkV3JpdGUgTm90ZXMuUmVhZFdyaXRlLkFsbCBvcGVuaWQgUGVvcGxlLlJlYWQgUHJlc2VuY2UuUmVhZCBQcmVzZW5jZS5SZWFkLkFsbCBwcm9maWxlIFJlcG9ydHMuUmVhZC5BbGwgU2l0ZXMuUmVhZFdyaXRlLkFsbCBUYXNrcy5SZWFkV3JpdGUgVXNlci5SZWFkIFVzZXIuUmVhZEJhc2ljLkFsbCBVc2VyLlJlYWRXcml0ZSBVc2VyLlJlYWRXcml0ZS5BbGwgZW1haWwiLCJzaWduaW5fc3RhdGUiOlsiZHZjX21uZ2QiLCJkdmNfY21wIiwiZHZjX2RtamQiLCJrbXNpIl0sInN1YiI6IlZWUnNfd1pjNUlVc2hvRGN6VzdQS2dkLXBqVnppeHk1MDNSS0x0UjBMQzgiLCJ0ZW5hbnRfcmVnaW9uX3Njb3BlIjoiV1ciLCJ0aWQiOiI3MmY5ODhiZi04NmYxLTQxYWYtOTFhYi0yZDdjZDAxMWRiNDciLCJ1bmlxdWVfbmFtZSI6InYtdmFyc3NsQG1pY3Jvc29mdC5jb20iLCJ1cG4iOiJ2LXZhcnNzbEBtaWNyb3NvZnQuY29tIiwidXRpIjoiQk1mTjhGS3dRMFNnWV9SVGx5d0JBQSIsInZlciI6IjEuMCIsInhtc19zdCI6eyJzdWIiOiJHWEZjMjgyeUFMbXdadWZJM0RwTVpDeWsxNE5kbWd5ZkVkbm5INS1jSHdzIn0sInhtc190Y2R0IjoxMjg5MjQxNTQ3fQ.kiMtzYG8T0qZgRe0Le8R9IejOmNXYWF3F8cUsqzw9o4NB2noeCPj7LHYoUwZbuqunPoLpnS1JIytdVj_sytLB4ASG9Eu8q4pWR4aW38Dxk7j8hZjBY-r4bCnHGaffPBkOkjHbsXNe_3Xqae3WOO3IsbNluOX0PasG6s6cZb1jNjVrFzonkrAlFSSnV-F48qdjtY4zprsf-h1oUAIWBHKjmEP3XqF6D5mXNXn0tNDzMU1Pl1omw08bB6xiXbEzdqyq3sEQtQdlPrxfFi3b_Y1It-qZD81Uu9uvX_QybtCdF4g6hP31DIojGqe40WCJr2H7lhuIW8711t69K0HhbIhow";
        //    string profilePhotoUrl = string.Empty;
        //    try
        //    {
        //        string endpoint = $"{_configuration["UsersEndPoint"]}v-abjodh@microsoft.com/photo/$value";
        //        using (var client = new HttpClient())
        //        {
        //            using (var request = new HttpRequestMessage(HttpMethod.Get, endpoint))
        //            {
        //                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        //                using (HttpResponseMessage response = await client.SendAsync(request))
        //                {
        //                    if (response.IsSuccessStatusCode)
        //                    {
        //                        var photo = await response.Content.ReadAsStreamAsync();
        //                        try
        //                        {
        //                            var fileName = userid + ".png";
        //                            string imagePath = "D:\\TaskManagement\\TaskManagement\\TaskManagement\\wwwroot\\Images\\ProfilePictures\\";
        //                            if (!System.IO.Directory.Exists(imagePath))
        //                                System.IO.Directory.CreateDirectory(imagePath);
        //                            imagePath += fileName;
        //                            using (var fileStream = System.IO.File.Create(imagePath))
        //                            {
        //                                photo.Seek(0, SeekOrigin.Begin);
        //                                photo.CopyTo(fileStream);
        //                            }
        //                            profilePhotoUrl = _configuration["BaseUri"] + "/images/ProfilePictures/" + fileName;
        //                        }
        //                        catch (Exception ex)
        //                        {
        //                            profilePhotoUrl = GetDefaultProfilePicture();
        //                        }
        //                    }
        //                    else
        //                    {
        //                        profilePhotoUrl = GetDefaultProfilePicture();
        //                    }
        //                }
        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        profilePhotoUrl = GetDefaultProfilePicture();
        //    }
        //    return profilePhotoUrl;
        //}

        //private string GetDefaultProfilePicture()
        //{
        //    return _configuration["BaseUri"] + "/Images/Avatar.png";
        //}

        //public async Task<ActionResult> GetByStatus(string status)
        //{
        //    List<TaskDataEntity> taskDataEntity = await _taskDataRepository.GetUserTasksAsync("Gousia Begum");
        //    var list = taskDataEntity.Where(o => o.TaskStatus == status).ToList();

        //    return RedirectToAction("Index");
        //}
        #endregion

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

            if (titleFromPayload == "tabload")
            {
                taskInfo.description = "";
                ViewBag.callFromTab = titleFromPayload;
            }

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

        [Route("editTask/{taskId}/{fromTab?}")]
        public async Task<ActionResult> EditTask(Guid taskId, string fromTab)
        {
            TaskDataEntity taskdataEntity = await _taskDataRepository.GetTaskDetailsByTaskIDAsync(taskId);
            TaskAttachementsEntity taskAttachementsEntity = await _taskAttachementsRepository.GetTaskAttachementDetailsByTaskIDAsync(taskId);
            List<TaskActivityEntity> taskActivityEntity = await _taskActivityRepository.GetTaskActivityDetailsByTaskIDAsync(taskId);
            var sortedActivityList = taskActivityEntity.OrderByDescending(x => x.Timestamp).ToList();
            var taskList = (await DBHelper.GetPageLoadDataAsync(_configuration)).ListofTaskIDs;
            var userList = await DBHelper.GetListOfUser(_configuration);
            var listofTasks = this.GetTaskListSelectItems(taskList);
            ViewBag.priority = taskdataEntity.TaskPriority;
            ViewBag.assignedTo = taskdataEntity.TaskAssignedTo;
            ViewBag.status = taskdataEntity.TaskStatus;
            ViewBag.title = taskdataEntity.TaskTitle;
            ViewBag.description = taskdataEntity.TaskDescription;
            ViewBag.subscribers = string.Join(",", taskdataEntity.Subscribers);
            ViewBag.dependetOn = string.Join(",", taskdataEntity.Dependencies);
            ViewBag.blocks = string.Join(",", taskdataEntity.Blocks);

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

            if (fromTab != null)
            {
                ViewBag.callFromTab = fromTab;
            }
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