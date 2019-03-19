using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using TimeTrackerServer.Models;

namespace TimeTrackerServer.Services
{
    public class ReportService
    {
        public IList<ReportDisplayInfo> GetTwoMonthReportInfo(int? customerId, string reportDate, int userId, int? sortType)
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
                        .GroupBy(a => new {a.ProjectID, a.TeamID})
                    join project in db.T_PM_Project.Where(a => a.CustomerID == customerId) on reportRecords.Key
                            .ProjectID equals
                        project.ProjectID into pro
                    from i in pro.DefaultIfEmpty()
                    where i.ProjectName != null
                    join member in db.T_PM_Member.Where(a =>
                        a.Charge.Equals(SystemConst.StatusCharge) && a.UserID == userId) on reportRecords.Key
                        .ProjectID equals member.ProjectID
                    join team in db.T_SD_CustomerTeam on reportRecords.Key.TeamID equals team.TeamID
                    select new ReportInfo
                    {
                        ProjectName = i.ProjectName,
                        TeamId = team.TeamID,
                        TeamName = team.TeamName,
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
                        .GroupBy(a => new {a.ProjectID, a.TeamID})
                    join team in db.T_SD_CustomerTeam on reportRecords.Key.TeamID equals team.TeamID
                    join project in db.T_PM_Project.Where(a => a.CustomerID == customerId) on reportRecords.Key.ProjectID equals
                        project.ProjectID into pro
                    from i in pro.DefaultIfEmpty()
                    where i.ProjectName != null
                    join member in db.T_PM_Member.Where(a =>
                            a.Charge.Equals(SystemConst.StatusCharge) && a.UserID == userId) on reportRecords.Key.ProjectID equals
                        member.ProjectID
                    select new ReportInfo
                    {
                        ProjectId = i.ProjectID,
                        ProjectName = i.ProjectName,
                        TeamId = team.TeamID,
                        TeamName = team.TeamName,
                        TaskNum = reportRecords.Sum(t => t.RealTask),
                        RealTime = reportRecords.Sum(t => t.TaskTime),
                        ModifyTime = reportRecords.Sum(t => t.ReportTime),
                    }).ToList();
                currentInfos.ForEach(a =>
                    a.ModifyAvg = a.TaskNum == 0
                        ? 0
                        : Math.Round(Convert.ToDouble(a.ModifyTime) / Convert.ToDouble(a.TaskNum), 2));
                var result = (from currentInfo in currentInfos
                    join reportInfo in reportInfos
                        on  new
                        {
                            currentInfo.ProjectName,
                            currentInfo.TeamName
                        } equals new
                        {
                            reportInfo.ProjectName,
                            reportInfo.TeamName
                        }
                        into tmp
                    from lastInfo in tmp.DefaultIfEmpty()
                    select new ReportDisplayInfo
                    {
                        ProjectId = currentInfo.ProjectId ?? 0,
                        ProjectName = currentInfo.ProjectName ?? string.Empty,
                        TeamId = currentInfo.TeamId,
                        TeamName = currentInfo.TeamName,
                        CurrentTask = currentInfo.TaskNum == null ? 0 : Convert.ToInt32(currentInfo.TaskNum),
                        CurrentTime = currentInfo.RealTime == null ? 0 : Convert.ToInt32(currentInfo.RealTime),
                        CurrentModify = currentInfo.ModifyTime == null ? 0 : Convert.ToInt32(currentInfo.ModifyTime),
                        CurrentAvg = currentInfo.ModifyAvg,
                        LastTask = lastInfo?.TaskNum == null ? 0 : Convert.ToInt32(lastInfo.TaskNum),
                        LastTime = lastInfo?.RealTime == null
                            ? 0
                            : Convert.ToInt32(lastInfo.RealTime),
                        LastModify = lastInfo?.ModifyTime == null
                            ? 0
                            : Convert.ToInt32(lastInfo.ModifyTime),
                        LastAvg = lastInfo?.ModifyAvg ?? 0,
                    }).ToList();
                switch (sortType)
                {
                    case (int)SortType.Project:
                        result = result.OrderBy(a => a.ProjectName).ToList();
                        break;
                    case (int)SortType.Team:
                        result = result.OrderBy(a => a.TeamName).ToList();
                        break;
                    case (int)SortType.Tasks:
                        result = result.OrderBy(a => a.CurrentTask).ToList();
                        break;
                    case (int)SortType.Time:
                        result = result.OrderBy(a => a.CurrentTime).ToList();
                        break;
                    case (int)SortType.Modify:
                        result = result.OrderBy(a => a.CurrentModify).ToList();
                        break;
                    case (int)SortType.Avg:
                        result = result.OrderBy(a => a.CurrentAvg).ToList();
                        break;
                    default:
                        break;
                }
                return result;
            }
        }

        public static void ModifyProjectTime(int projectId, int teamId, int changeTime, string month)
        {
            if (changeTime == 0)
                return;
            using (var db = new TimeTrackerEntities())
            {
                string[] date = month.Split('-');
                int year = Convert.ToInt16(date[0]);
                int mon = Convert.ToInt16(date[1]);
                var allProjectTasks = db.T_PM_Task.Where(a =>
                        a.ProjectID == projectId && a.TeamID == teamId && a.TaskDate.Year == year &&
                        a.TaskDate.Month == mon)
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

        public static IList<CustomerProjectDetail> GetCustomerProjectDetails(int customerId, string reportDate, int userId)
        {
            using (var db = new TimeTrackerEntities())
            {
                DateTime reportMonth = reportDate == null || reportDate.Equals(string.Empty)
                    ? DateTime.Now
                    : Convert.ToDateTime(reportDate);
                var customerProjectDetails = (from tasks in db.T_PM_Task.Where(a =>
                        a.TaskDate.Year == reportMonth.Year && a.TaskDate.Month == reportMonth.Month)
                    join teams in db.T_SD_CustomerTeam.Where(a => a.CustomerID == customerId) on tasks.TeamID equals
                        teams.TeamID
                    join projects in db.T_PM_Project.Where(a => a.CustomerID == customerId) on tasks.ProjectID equals
                        projects.ProjectID
                    join member in db.T_PM_Member.Where(a =>
                            a.Charge.Equals(SystemConst.StatusCharge) && a.UserID == userId) on tasks.ProjectID equals
                        member.ProjectID
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

        public static List<List<string>> GetDisplayCustomerProjectDetails(
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
            title.Add("Avg Speed");

            result.Add(title);
            List<string> projects = new List<string>();
            foreach (CustomerProjectDetail customerProjectDetail in customerProjectDetails)
            {
                List<string> line = null;
                if (!projects.Contains(customerProjectDetail.ProjectName))
                {
                    projects.Add(customerProjectDetail.ProjectName);
                    line = new List<string> {customerProjectDetail.ProjectName};
                    for (int i = 0; i < title.Count - 2; i++)
                    {
                        line.Add("");
                    }

                    result.Add(line);
                }
                else
                {
                    line = result[projects.IndexOf(customerProjectDetail.ProjectName) + 1];
                }

                if (customerProjectDetail.TaskCount != null && customerProjectDetail.TaskCount != 0)
                {
                    line[title.IndexOf(customerProjectDetail.TeamName)] = customerProjectDetail.TaskCount.ToString();
                }
            }

            for (int i = 1; i < result.Count; i++)
            {
                List<string> line = result[i];
                string projectName = line[0];
                List<CustomerProjectDetail> selectedCustomerProjectDetails = customerProjectDetails.Where(a => a.ProjectName.Equals(projectName)).ToList();
                int? x = selectedCustomerProjectDetails.Sum(a => a.TaskCount);
                int? y = selectedCustomerProjectDetails.Sum(a => a.TimeCount);
                line.Add(Math.Round(Convert.ToDouble(y) / Convert.ToDouble(x), 2).ToString());
            }

            int volumnSummary = Convert.ToInt32(customerProjectDetails.Sum(a => a.TaskCount));
            List<string> volume = new List<string>
            {
                "Volume Percent"
            };
            result.Add(volume);

            int timeSummary = Convert.ToInt32(customerProjectDetails.Sum(a => a.TimeCount));
            List<string> time = new List<string>
            {
                "Time Percent"
            };
            result.Add(time);

            for (int i = 1; i < title.Count - 1; i++)
            {
                string teamName = title[i];
                volume.Add((Convert.ToDouble(customerProjectDetails.Where(a => a.TeamName.Equals(teamName))
                                .Sum(a => a.TaskCount)) / volumnSummary).ToString("P"));
                time.Add((Convert.ToDouble(customerProjectDetails.Where(a => a.TeamName.Equals(teamName))
                              .Sum(a => a.TimeCount)) / timeSummary).ToString("P"));
            }

            List<string> total = new List<string>
            {
                "Total"
            };
            result.Add(total);
            for (int i = 1; i < title.Count; i++)
            {
                string teamName = title[i];
                int? taskCount = customerProjectDetails.Where(a => a.TeamName.Equals(teamName)).Sum(a => a.TaskCount);
                int? timeCount = customerProjectDetails.Where(a => a.TeamName.Equals(teamName)).Sum(a => a.TimeCount);
                if (timeCount != 0)
                    total.Add(taskCount.ToString());
            }
            
            volume.Add("");
            time.Add("");
            total.Add("");
            return result;
        }
    }
}