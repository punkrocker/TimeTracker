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
        //select t1.ProjectID,
        //t1.ProjectName,
        //t2.UserID,
        //t3.UseName,
        //t2.Charge,
        //isnull(t4.task, 0) as Task,
        //isnull(t4.time, 0) as Time

        //    from T_PM_Project t1
        //left join T_PM_Member t2 on t1.ProjectID = t2.ProjectID
        //    left join T_Sys_UserInfo t3 on t2.UserID = t3.UserID
        //    left join (select UserID, sum(RealTask) as task, sum(ReportTime) as time
        //    from T_PM_Task
        //    where TaskDate = CONVERT(varchar(10), GETDATE(), 120)
        //group by UserID) t4 on t3.UserID = t4.UserID

        //    where t1.ProjectID in (select ProjectID from T_PM_Member where UserID = 32 and Charge = 1);
        public static IList<ProjectSubmitInfo> GetProjectSubmitInfos(int userId)
        {
            using (var db = new TimeTrackerEntities())
            {
                var projects = (from project in db.T_PM_Project
                    join member in db.T_PM_Member.Where(a => a.UserID == userId && a.Charge.Equals(SystemConst.StatusCharge))
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
    }
}