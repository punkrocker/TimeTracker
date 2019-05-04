using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Model;
using TimeTrackerServer.Models;
using TimeTrackerServer.Services;

namespace TimeTrackerServer.Controllers
{
    public class ProjectController : Controller
    {
        // GET: Project
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Submit()
        {
            int userId = Convert.ToInt32(Session["UserID"]);
            List<ProjectSubmitInfo> submitInfos = ProjectService.GetProjectSubmitInfos(userId).ToList();
            ViewBag.model = submitInfos;
            return View();
        }

        public ActionResult SubmitDetail(int projectId)
        {
            return View();
        }

    }
}