using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Model;
using Model.Dto;
using Newtonsoft.Json;
using TimeTrackerServer.App_Start;
using TimeTrackerServer.Services;

namespace TimeTrackerServer.Controllers
{
    [Login]
    public class ReportController : Controller
    {
        // GET: Report
        public ActionResult Index(int? customerId, string reportDate)
        {
            ReportService reportService = new ReportService();
            ViewBag.model = reportService.GetReportDisplayInfos(customerId, reportDate);
            ViewBag.customers = CustomerService.GetAllCustomers();
            return View();
        }

        [HttpPost]
        public ActionResult Report(List<ReportDisplayInfo> reportList, string comment, int customerId, string currentMonth)
        {
            List<String> projectNames = new List<string>();
            List<Int32> lastTasks = new List<Int32>();
            List<Int32> currentTasks = new List<Int32>();
            List<Int32> lastTimes = new List<Int32>();
            List<Int32> currentTimes = new List<Int32>();
            List<CustomerTeamsDto> customerTeams = CustomerService.GetCustomerTeams(customerId).ToList();
            List<T_PM_Project> cc = CustomerService.GetCustomerProjects(customerId).ToList();
            List<CustomerProjectDetail> customerProjectDetails = ReportService.GetCustomerProjectDetails(customerId, currentMonth).ToList();
            foreach (ReportDisplayInfo reportDisplayInfo in reportList)
            {
                projectNames.Add(reportDisplayInfo.ProjectName);
                lastTasks.Add(reportDisplayInfo.LastTask);
                currentTasks.Add(reportDisplayInfo.CurrentTask);
                lastTimes.Add(reportDisplayInfo.LastModify);
                currentTimes.Add(reportDisplayInfo.CurrentModify);
            }

            ViewBag.projectNames = projectNames;
            ViewBag.lastTasks = lastTasks;
            ViewBag.currentTasks = currentTasks;
            ViewBag.sumTask = currentTasks.Sum();
            ViewBag.lastTimes = lastTimes;
            ViewBag.currentTimes = currentTimes;
            ViewBag.sumTimes = currentTimes.Sum();
            ViewBag.comment = comment;
            ViewBag.customerTeams = customerTeams;
            ViewBag.currentMonth = currentMonth;
            ViewBag.sumAllProjects = cc;
            ViewBag.customerProjectDetails = customerProjectDetails;
            return View();
        }

        [HttpPost]
        public ActionResult ModifyReport(int projectID, Int32 modifyTime)
        {
            ReportService.ModifyProjectTime(projectID, modifyTime);
            return View();
        }
    }
}