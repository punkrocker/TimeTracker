using System;
using System.Collections.Generic;
using System.IO;
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
        public ActionResult Index(int? customerId, string reportDate, int? sortType)
        {
            ReportService reportService = new ReportService();
            ViewBag.model = reportService.GetTwoMonthReportInfo(customerId,
                reportDate,
                Convert.ToInt16(Session["UserId"]),
                sortType);
            ViewBag.customers = CustomerService.GetCustomerByUser(Convert.ToInt16(Session["UserID"]));
            return View();
        }

        [HttpPost]
        public ActionResult Report(List<ReportDisplayInfo> reportList, string comment, int customerId, string currentMonth)
        {
            List<ReportDisplayInfo> groupedReportList = new List<ReportDisplayInfo>();
            reportList.ForEach(c =>
            {
                var currentReport = groupedReportList.Where(a => a.ProjectName.Equals(c.ProjectName)).FirstOrDefault();
                if (currentReport == null)
                    groupedReportList.Add(c);
                else
                {
                    currentReport.CurrentModify += c.CurrentModify;
                    currentReport.CurrentTask += c.CurrentTask;
                    currentReport.CurrentTime += c.CurrentTime;
                    currentReport.LastModify += c.LastModify;
                    currentReport.LastTask += c.LastTask;
                    currentReport.LastTime += c.LastTime;
                }
            });

            List<String> projectNames = new List<string>();
            List<Int32> lastTasks = new List<Int32>();
            List<Int32> currentTasks = new List<Int32>();
            List<Int32> lastTimes = new List<Int32>();
            List<Int32> currentTimes = new List<Int32>();
            List<CustomerTeamsDto> customerTeams = CustomerService.GetCustomerTeams(customerId).ToList();
            List<CustomerProjectDetail> customerProjectDetails = ReportService.GetCustomerProjectDetails(customerId, currentMonth, Convert.ToInt16(Session["UserId"])).ToList();
            List<List<string>> displayTableInfo = ReportService.GetDisplayCustomerProjectDetails(customerProjectDetails);
            foreach (ReportDisplayInfo reportDisplayInfo in groupedReportList)
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
            ViewBag.customerProjectDetails = displayTableInfo;
            return View();
        }

        [HttpPost]
        public ActionResult ModifyReport(int projectID, int teamID, Int32 modifyTime, string month)
        {
            ReportService.ModifyProjectTime(projectID, teamID, modifyTime, month);
            return View();
        }
    }
}