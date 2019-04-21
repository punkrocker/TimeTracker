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
        public IList<ProjectSubmitInfo> GetProjectSubmitInfos(int userId)
        {
            using (var db = new TimeTrackerEntities())
            {
                var taskSummary = (from task in db.T_PM_Task
                        .Where(a => a.TaskDate.Year == DateTime.Today.Year
                                    && a.TaskDate.Month == DateTime.Today.Month
                                    && a.TaskDate.Day == DateTime.Today.Day)
                        .GroupBy(a => new
                        {
                            a.UserID
                        })
                    select new T_PM_Task
                    {
                        UserID = task.Key.UserID,
                        RealTask = task.Sum(a => a.RealTask),
                        ReportTime = task.Sum(a => a.ReportTime)
                    });
                var projectSubmitInfos = (from project in db.T_PM_Project
                    join member in db.T_PM_Member on project.ProjectID equals member.ProjectID
                    join user in db.T_Sys_UserInfo on member.UserID equals user.UserID
                    join task in taskSummary on member.UserID equals task.UserID
                    select new ProjectSubmitInfo
                    {
                        ProjectID = project.ProjectID,
                        ProjectName = project.ProjectName,
                        UserID = user.UserID,
                        UseName = user.UseName,
                        Task = Convert.ToInt32(task.RealTask),
                        Time = Convert.ToInt32(task.ReportTime)
                    }).ToList();
                return projectSubmitInfos;
            }
        }
    }
}