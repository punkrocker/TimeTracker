using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Model;
using Model.Dto;
using Model.WebResult;
using Newtonsoft.Json;
using WebGrease.Css.Ast.Selectors;

namespace TimeTrackerServer.Controllers
{
    public class ApiController : Controller
    {
        [HttpPost]
        public ActionResult Login(LoginPara para)
        {
            WebResult result = new WebResult()
            {
                Code = SystemConst.MsgErrUnknown,
                Message = String.Empty
            };

            try
            {
                using (var db = new TimeTrackerEntities())
                {
                    var password =
                        System.Web.Security.FormsAuthentication
                            .HashPasswordForStoringInConfigFile(para.Password, "MD5");
                    var user = db.T_Sys_UserInfo.FirstOrDefault(a => a.UseName.Equals(para.UserName));
                    if (user.UserPwd.Equals(password))
                    {
                        result.Code = SystemConst.MsgSuccess;
                        result.Message = user.UserID.ToString();
                    }
                    else
                    {
                        result.Message = "ERR_PWD";
                    }
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return Content(AppUtils.JsonSerializer(result));
        }

        public ActionResult GetProjects(int userID)
        {
            UserProjectResult result = new UserProjectResult()
            {
                Code = SystemConst.MsgErrUnknown,
                Message = String.Empty,
                Data = null
            };

            try
            {
                using (var db = new TimeTrackerEntities())
                {
                    var user = db.T_Sys_UserInfo.Where(a => a.UserID == userID).FirstOrDefault();
                    if (user == null || user.Status == SystemConst.StatusDisable)
                        return Content(AppUtils.JsonSerializer(result));
                    var usersProjects =
                        (from projects in db.T_PM_Project.Where(a => a.Status == SystemConst.StatusIssued)
                         join customer in db.T_SD_Customer.Where(a=>a.Status==SystemConst.StatusEnable) on projects.CustomerID equals customer.CustomerID
                            join members in db.T_PM_Member on projects.ProjectID equals members.ProjectID
                                into memberProjects
                            from memberProject in memberProjects.DefaultIfEmpty().Where(a => a.UserID == userID)
                            select new UserProjectsDto
                            {
                                ProjectId = projects.ProjectID,
                                ProjectCode = projects.ProjectCode,
                                ProjectName = projects.ProjectName,
                                CustomerId = projects.CustomerID,
                            }).ToList();
                    result.Code = SystemConst.MsgSuccess;
                    result.Data = usersProjects;
                }
            }
            catch (Exception e)
            {
                result.Message = e.Message;
            }

            return Content(AppUtils.JsonSerializer(result));
        }

        public ActionResult GetCustomerTeams(int customerID)
        {
            CustomerTeamsResult result = new CustomerTeamsResult()
            {
                Code = SystemConst.MsgErrUnknown,
                Message = String.Empty,
                Data = null
            };
            try
            {
                using (var db = new TimeTrackerEntities())
                {
                    var customerTeams = (from teams in db.T_SD_CustomerTeam.Where(a =>
                            a.CustomerID == customerID && a.Status == SystemConst.StatusEnable)
                        join customer in db.T_SD_Customer on teams.CustomerID equals customer.CustomerID
                            into customerTeamInfos
                        from customerTeamInfo in customerTeamInfos.DefaultIfEmpty().Where(a =>
                            a.CustomerID == customerID && a.Status == SystemConst.StatusEnable)
                        select new CustomerTeamsDto
                        {
                            CustomerName = customerTeamInfo.CustomerName,
                            TeamId = teams.TeamID,
                            TeamName = teams.TeamName
                        }).ToList();
                    result.Code = SystemConst.MsgSuccess;
                    result.Data = customerTeams;
                }
            }
            catch (Exception e)
            {
                result.Message = e.Message;
            }

            return Content(AppUtils.JsonSerializer(result));
        }

        [HttpPost]
        public ActionResult CommitProjects(List<CommitTask> commitTasks)
        {
            WebResult result = new WebResult()
            {
                Code = SystemConst.MsgErrUnknown,
                Message = String.Empty
            };

            try
            {
                using (var db = new TimeTrackerEntities())
                {
                    foreach (var commitTask in commitTasks)
                    {
                        var max = db.T_PM_Task.Where(a => a.ProjectID == commitTask.ProjectId &&
                                                          a.UserID == commitTask.UserId &&
                                                          a.TaskDate == commitTask.TaskDate &&
                                                          a.TeamID == commitTask.TeamId).Select(a => a.SubmitSeq)
                                      .DefaultIfEmpty().Max() + 1;

                        T_PM_Task test = new T_PM_Task
                        {
                            ProjectID = commitTask.ProjectId,
                            UserID = commitTask.UserId,
                            TaskDate = commitTask.TaskDate,
                            SubmitSeq = max,
                            TeamID = commitTask.TeamId,
                            PlanTask = Convert.ToInt32(commitTask.PlanTask),
                            RealTask = Convert.ToInt32(commitTask.RealTask),
                            TaskTime = Convert.ToInt32(commitTask.TaskTime),
                            Desc = commitTask.Desc == null ? string.Empty : commitTask.Desc,
                            SubmitTime = commitTask.SubmitTime,
                            ReportTime = Convert.ToInt32(commitTask.TaskTime),
                        };
                        db.T_PM_Task.Add(test);
                    }
                    db.SaveChanges();
                    result.Code = SystemConst.MsgSuccess;
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return Content(AppUtils.JsonSerializer(result));
        }
    }
}