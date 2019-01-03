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
        [HttpPost]
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

                
            }
            return Content(AppUtils.JsonSerializer(result));
        }

    }
}