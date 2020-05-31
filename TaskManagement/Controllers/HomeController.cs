using Microsoft.AspNetCore.Mvc;
using TaskManagement.Helper;


namespace TaskManagement.Controllers
{
    
    public class HomeController : Controller
    { 

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

        [Route("createNewTask")]
        public ActionResult CreateNewTask()
        {
            string taskId = Common.GetNewTaskID();
            ViewBag.newTaskID = taskId;
            return View();
        }     

        [Route("configure")]
        public ActionResult Configure()
        {
            return View();
        }
    
       

    }
}