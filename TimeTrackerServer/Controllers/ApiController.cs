using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Model;
using Model.Dto;
using Model.WebResult;

namespace TimeTrackerServer.Controllers
{
    public class ApiController : Controller
    {
        public ActionResult Login(LoginPara para)
        {
            WebResult result = new WebResult()
            {
                Code = SystemConst.MSG_ERR_UNKNOWN,
                Message = String.Empty
            };

            using (var db = new TimeTrackerEntities())
            {
                var password = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(para.Password, "MD5");
                var user = db.T_Sys_UserInfo.Where(a => a.UseName.Equals(para.UserName)).FirstOrDefault();
                if (user.UserPwd.Equals(password))
                {
                    result.Code = SystemConst.MSG_SUCCESS;
                }
                else
                {
                    result.Message = "ERR_PWD";
                }
            }
            return Content(AppUtils.JsonSerializer(result));
        }

    }
}