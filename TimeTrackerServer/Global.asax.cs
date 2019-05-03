using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace TimeTrackerServer
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }


        protected void Application_Error(object sender, EventArgs e)
        {
            var error = Server.GetLastError();
            var code = (error is HttpException) ? (error as HttpException).GetHttpCode() : 500;
            if (code != 404)
            {
                Response.Clear();
                Server.ClearError();
            }
            else
            {
                //防止302，对seo友好。
                var exception = Server.GetLastError();
                var httpException = exception as HttpException;
                Response.Clear();
                Server.ClearError();
                var routeData = new RouteData();
                routeData.Values["controller"] = "Error";
                routeData.Values["action"] = "PageNotFound";
                routeData.Values["exception"] = exception;
                Response.TrySkipIisCustomErrors = true;
                IController errorManager = new Controllers.ErrorController();
                HttpContextWrapper wrapper = new HttpContextWrapper(Context);
                var rc = new RequestContext(wrapper, routeData);
                errorManager.Execute(rc);
            }
        }
    }
}
