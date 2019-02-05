﻿using Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TimeTrackerServer.Services
{
    public class ReportService
    {
        public IList<ReportDisplayInfo> GetReportDisplayInfos(int? customerId, string reportDate)
        {
            if (customerId == null)
                return new List<ReportDisplayInfo>();
            using (var db = new TimeTrackerEntities())
            {
                DateTime reportMonth = reportDate == null || reportDate.Equals(string.Empty)
                    ? DateTime.Now
                    : Convert.ToDateTime(reportDate);
                DateTime lastMonth = reportMonth.AddMonths(-1);
                var reportInfos = (from reportRecords in db.T_PM_Task
                        .Where(a => a.TaskDate.Year == lastMonth.Year && a.TaskDate.Month == lastMonth.Month)
                        .GroupBy(a => a.ProjectID)
                    join project in db.T_PM_Project.Where(a => a.CustomerID == customerId) on reportRecords.Key equals
                        project
                            .ProjectID
                        into pro
                    from i in pro.DefaultIfEmpty()
                    where i.ProjectName != null
                    select new ReportInfo
                    {
                        ProjectName = i.ProjectName,
                        TaskNum = reportRecords.Sum(t => t.RealTask),
                        RealTime = reportRecords.Sum(t => t.TaskTime),
                        ModifyTime = reportRecords.Sum(t => t.ReportTime),
                    }).ToList();
                reportInfos.ForEach(a =>
                    a.ModifyAvg = a.TaskNum == 0
                        ? 0
                        : Math.Round(Convert.ToDouble(a.ModifyTime) / Convert.ToDouble(a.TaskNum), 2));
                var currentInfos = (from reportRecords in db.T_PM_Task
                        .Where(a => a.TaskDate.Year == reportMonth.Year && a.TaskDate.Month == reportMonth.Month)
                        .GroupBy(a => a.ProjectID)
                    join project in db.T_PM_Project.Where(a => a.CustomerID == customerId) on reportRecords.Key equals
                        project
                            .ProjectID
                        into pro
                    from i in pro.DefaultIfEmpty()
                    where i.ProjectName != null
                    select new ReportInfo
                    {
                        ProjectId = i.ProjectID,
                        ProjectName = i.ProjectName,
                        TaskNum = reportRecords.Sum(t => t.RealTask),
                        RealTime = reportRecords.Sum(t => t.TaskTime),
                        ModifyTime = reportRecords.Sum(t => t.ReportTime),
                    }).ToList();
                currentInfos.ForEach(a =>
                    a.ModifyAvg = a.TaskNum == 0
                        ? 0
                        : Math.Round(Convert.ToDouble(a.ModifyTime) / Convert.ToDouble(a.TaskNum), 2));
                var result = (from currentInfo in currentInfos
                    join reportInfo in reportInfos on currentInfo.ProjectName equals reportInfo.ProjectName into tmp
                    from lastInfo in tmp.DefaultIfEmpty()
                    select new ReportDisplayInfo
                    {
                        ProjectId = currentInfo.ProjectId == null ? 0 : currentInfo.ProjectId,
                        ProjectName = currentInfo.ProjectName == null ? string.Empty : currentInfo.ProjectName,
                        CurrentTask = currentInfo.TaskNum == null ? 0 : Convert.ToInt32(currentInfo.TaskNum),
                        CurrentTime = currentInfo.RealTime == null ? 0 : Convert.ToInt32(currentInfo.RealTime),
                        CurrentModify = currentInfo.ModifyTime == null ? 0 : Convert.ToInt32(currentInfo.ModifyTime),
                        CurrentAvg = currentInfo.ModifyAvg == null ? 0 : currentInfo.ModifyAvg,
                        LastTask = lastInfo == null || null == lastInfo.TaskNum ? 0 : Convert.ToInt32(lastInfo.TaskNum),
                        LastTime = lastInfo == null || null == lastInfo.RealTime
                            ? 0
                            : Convert.ToInt32(lastInfo.RealTime),
                        LastModify = lastInfo == null || null == lastInfo.ModifyTime
                            ? 0
                            : Convert.ToInt32(lastInfo.ModifyTime),
                        LastAvg = lastInfo == null || null == lastInfo.ModifyAvg
                            ? 0
                            : lastInfo.ModifyAvg,
                    }).ToList();
                return result;
            }
        }

        public static void ModifyProjectTime(int projectId, Int32 changeTime)
        {
            using (var db = new TimeTrackerEntities())
            {
                DateTime today = DateTime.Now;
                var allProjectTasks = db.T_PM_Task.Where(a =>
                        a.ProjectID == projectId && a.TaskDate.Year == today.Year && a.TaskDate.Month == today.Month)
                    .ToList();
                int index = 0;
                //获取变化量为增还是为减
                int posNeg = changeTime / Math.Abs(changeTime);
                //只有全部时间都处理完才跳出循环
                while (changeTime != 0)
                {
                    if (allProjectTasks[index].RealTask == null)
                        continue;
                    int taskNum = Convert.ToInt32(allProjectTasks[index].RealTask);
                    if (Math.Abs(changeTime) > taskNum)
                    {
                        allProjectTasks[index].ReportTime += (taskNum * posNeg);
                        changeTime -= taskNum * posNeg;
                    }
                    else
                    {
                        allProjectTasks[index].ReportTime += changeTime;
                        break;
                    }

                    //选取下一个task，如果下标与所有task总数一致，则从头开始重新循环
                    index++;
                    if (index == allProjectTasks.Count)
                        index = 0;
                }

                foreach (T_PM_Task allProjectTask in allProjectTasks)
                {
                    db.T_PM_Task.Attach(allProjectTask);
                    db.Entry(allProjectTask).State = System.Data.Entity.EntityState.Modified;
                }

                db.SaveChanges();
            }
        }

        public static IList<CustomerProjectDetail> GetCustomerProjectDetails(int customerId, string reportDate)
        {
            using (var db = new TimeTrackerEntities())
            {
                DateTime reportMonth = reportDate == null || reportDate.Equals(string.Empty)
                    ? DateTime.Now
                    : Convert.ToDateTime(reportDate);
                DateTime lastMonth = reportMonth.AddMonths(-1);
                var customerProjectDetails = (from tasks in db.T_PM_Task.Where(a =>
                        a.TaskDate.Year == reportMonth.Year && a.TaskDate.Month == reportMonth.Month)
                    join teams in db.T_SD_CustomerTeam.Where(a => a.CustomerID == customerId) on tasks.TeamID equals
                        teams.TeamID
                    join projects in db.T_PM_Project.Where(a => a.CustomerID == customerId) on tasks.ProjectID equals
                        projects.ProjectID
                    group new
                    {
                        teams.TeamName,
                        projects.ProjectName,
                        tasks.ReportTime,
                        tasks.RealTask
                    } by new
                    {
                        teams.TeamName,
                        projects.ProjectName,
                    }
                    into g
                    select new CustomerProjectDetail
                    {
                        ProjectName = g.Key.ProjectName,
                        TeamName = g.Key.TeamName,
                        TaskCount = g.Sum(t => t.RealTask),
                        TimeCount = g.Sum(t => t.ReportTime),
                    }).ToList();
                return customerProjectDetails;
            }
        }

        public static Dictionary<string, CustomerProjectDetail> GetDisplayCustomerProjectDetails(
            IList<CustomerProjectDetail> customerProjectDetails)
        {
            Dictionary<string, CustomerProjectDetail> result = new Dictionary<string, CustomerProjectDetail>();
            foreach (var customerProjectDetail in customerProjectDetails)
            {
                result.Add(customerProjectDetail.ProjectName + "-" + customerProjectDetail.TeamName,
                    customerProjectDetail);
            }

            return result;
        }

        public static List<List<string>> GetDisplayCustomerProjectDetails2(
            IList<CustomerProjectDetail> customerProjectDetails)
        {
            List<List<string>> result = new List<List<string>>();
            List<string> title = new List<string>
            {
                "PN/TN"
            };
            foreach (CustomerProjectDetail customerProjectDetail in customerProjectDetails)
            {
                if (!title.Contains(customerProjectDetail.TeamName))
                    title.Add(customerProjectDetail.TeamName);

            }

            result.Add(title);
            List<string> projects = new List<string>();
            foreach (CustomerProjectDetail customerProjectDetail in customerProjectDetails)
            {
                List<string> line = null;
                if (!projects.Contains(customerProjectDetail.ProjectName))
                {
                    projects.Add(customerProjectDetail.ProjectName);
                    line = new List<string> {customerProjectDetail.ProjectName};
                    foreach (var t in title)
                    {
                        line.Add("");
                    }

                    result.Add(line);
                }
                else
                {
                    line = result[projects.IndexOf(customerProjectDetail.ProjectName) + 1];
                }

                //line.Add(customerProjectDetail.TimeCount.ToString() + "/" + customerProjectDetail.TaskCount.ToString());
            }

            return result;
        }
    }
}