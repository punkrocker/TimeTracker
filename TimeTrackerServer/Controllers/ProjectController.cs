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
            List<ProjectSubmitInfo> submitInfos = ProjectService.GetProjectSubmitInfos(32).ToList();
            ViewBag.model = submitInfos;
            return View();
        }
    }
}