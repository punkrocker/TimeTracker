using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Model;
using TimeTrackerServer.Models;

namespace TimeTrackerServer.Services
{
    public class ProjectService
    {
        public static IList<ProjectSubmitInfo> GetProjectSubmitInfos(int userId)
        {
            using (var db = new TimeTrackerEntities())
            {
                var projects = (from project in db.T_PM_Project
                    join member in db.T_PM_Member.Where(a =>
                            a.UserID == userId && a.Charge.Equals(SystemConst.StatusCharge))
                        on project.ProjectID equals member.ProjectID
                    select new SimpleProject
                    {
                        ProjectId = project.ProjectID,
                        ProjectName = project.ProjectName
                    }).ToList();
                var taskSummary = (from task in db.T_PM_Task
                        .Where(a => a.TaskDate.Year == DateTime.Today.Year
                                    && a.TaskDate.Month == DateTime.Today.Month
                                    && a.TaskDate.Day == DateTime.Today.Day)
                        .GroupBy(a => new
                        {
                            a.UserID,
                            a.ProjectID
                        })
                    select new TaskSummary
                    {
                        UserId = task.Key.UserID,
                        ProjectId = task.Key.ProjectID,
                        RealTask = task.Sum(a => a.RealTask),
                        ReportTime = task.Sum(a => a.ReportTime)
                    });
                var projectSubmitInfos = (from project in projects
                    join member in db.T_PM_Member on project.ProjectId equals member.ProjectID
                    join user in db.T_Sys_UserInfo on member.UserID equals user.UserID
                    join task in taskSummary on new {UserId = member.UserID, ProjectId = member.ProjectID} equals new
                        {task.UserId, task.ProjectId}
                    select new ProjectSubmitInfo
                    {
                        ProjectID = project.ProjectId,
                        ProjectName = project.ProjectName,
                        UserID = user.UserID,
                        UseName = user.UseName,
                        Task = task.RealTask,
                        Time = task.ReportTime
                    }).ToList();
                return projectSubmitInfos.ToList();
            }
        }

        public static string GetProjectName(int projectId)
        {
            using (var db = new TimeTrackerEntities())
            {
                var project = db.T_PM_Project.FirstOrDefault(a => a.ProjectID == projectId);
                return project.ProjectName;
            }
        }
    }
}