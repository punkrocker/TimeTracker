using System.Web.Mvc;

namespace TimeTrackerServer.Controllers
{
    public class ErrorController : Controller
    {
        // GET
        public ActionResult PageNotFound()
        {
            return View();
        }
    }
}