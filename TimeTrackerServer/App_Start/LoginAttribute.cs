using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TimeTrackerServer.App_Start
{
    public class LoginAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            //return base.AuthorizeCore(httpContext);
            return !Convert.ToString(httpContext.Session["UserId"]).Equals(string.Empty);
        }
    }
}